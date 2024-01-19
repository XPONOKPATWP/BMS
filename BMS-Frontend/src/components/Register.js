// src/components/Register.js
import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { setMessage, clearMessage } from '../redux/feedback';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';
import axios from 'axios';

function Register() {
  const [name, setName] = useState(''); // Add name state
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState(''); // Add confirmPassword state
  const dispatch = useDispatch();
  const feedback = useSelector((state) => state.feedback);
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();

    // Validation
    if (!name || !email || !password || !confirmPassword) {
      dispatch(setMessage({ message: 'All fields are required', type: 'error' }));
      return;
    }

    if (password !== confirmPassword) {
      dispatch(setMessage({ message: 'Passwords do not match', type: 'error' }));
      return;
    }

    try {
      const response = await axios.post('https://localhost:7023/User/register', {
      name,
      email,
      password,
      confirmPassword,
    }, {
      headers: {
        'Content-Type': 'application/json',
      },
    });
      if (response.status === 200) {
        dispatch(clearMessage());
        console.log('Registration successful');
        dispatch(setMessage({ message: 'Registration Successfull', type: '' }));
        setTimeout(() => {
          navigate('/login');
          console.log('Redirecting to login page...');
        }, 2000);
      }
    } catch (error) {
      console.error('Registration failed:', error);
      dispatch(setMessage({ message: 'Registration failed', type: 'error' }));
    }
  };

  return (
    <div className="container mx-auto mt-5">
      <h1 className="text-2xl font-bold mb-4">Register</h1>
      {feedback.message && (
        <div className={`text-${feedback.type}-500 mb-4`}>{feedback.message}</div>
      )}
      <form onSubmit={handleRegister}>
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="name">
            Name
          </label>
          <input
            type="text"
            id="name"
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            placeholder="Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </div>
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="email">
            Email
          </label>
          <input
            type="email"
            id="email"
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="password">
            Password
          </label>
          <input
            type="password"
            id="password"
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </div>
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="confirmPassword">
            Confirm Password
          </label>
          <input
            type="password"
            id="confirmPassword"
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            placeholder="Confirm Password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
          />
        </div>
        <div className="flex items-center justify-between">
          <button
            type="submit"
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline"
          >
            Register
          </button>
        </div>
      </form>
      <div className="text-sm mt-10">
        Already have an account? <Link to="/login" className="text-blue-500 hover:underline">Login here</Link>
      </div>
    </div>
  );
}

export default Register;
