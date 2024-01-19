// src/components/Login.js
import React, { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { loginSuccess } from '../redux/auth';
import { setMessage, clearMessage } from '../redux/feedback';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';

function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const dispatch = useDispatch();
  const feedback = useSelector((state) => state.feedback);

  const navigate = useNavigate();

  
  const handleLogin = async (e) => {
    e.preventDefault();
    try {
      const response = await axios.post('https://localhost:7023/User/login', {
      email,
      password,
    }, {
      headers: {
        'Content-Type': 'application/json',
      },
    });
      const token = response.data.token;
      localStorage.setItem('authToken', token);
      dispatch(loginSuccess(token));
      dispatch(clearMessage());
      if (response.status === 200) {
        dispatch(clearMessage());
        console.log('Login successful');
        navigate('/profile');  // redirects to Profile page
        // Redirect to the login page or handle success as needed
      }
      // Save the token to the session or localStorage as needed
    } catch (error) {
      console.error('Login failed:', error);
      dispatch(setMessage({ message: 'Login failed', type: 'error' }));
    }
  };

  return (
    <div className="container mx-auto mt-5">
      <h1 className="text-2xl font-bold mb-4">Login</h1>
      {feedback.message && (
        <div className={`text-${feedback.type}-500 mb-4`}>{feedback.message}</div>
      )}
      <form onSubmit={handleLogin}>
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
        <div className="flex items-center justify-between">
          <button
            type="submit"
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline"
          >
            Login
          </button>
        </div>
      </form>
      <div className="text-sm mt-10">
        Don't have an account? <Link to="/register" className="text-blue-500 hover:underline">Register here</Link>
      </div>
    </div>
  );
}

export default Login;
