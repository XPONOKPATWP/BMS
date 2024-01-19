import React, { useState, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { setMessage, clearMessage } from '../redux/feedback';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

function Profile() {
  const feedback = useSelector((state) => state.feedback);
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const dispatch = useDispatch();
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthenticated) {
        navigate('/login');
    }
    const fetchProfile = async () => {
      try {
        const token = localStorage.getItem('authToken');
        const response = await axios.get('https://localhost:7023/User/Profile', {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': token,
          },
        });
        if (response.status === 200) {
          setName(response.data.name);
          setEmail(response.data.email);
        }
      } catch (error) {
        console.error('Fetching profile failed:', error);
        dispatch(setMessage({ message: 'Fetching profile failed', type: 'error' }));
      }
    };
    fetchProfile();
  }, [isAuthenticated, navigate, dispatch]);



  const handleUpdate = async (e) => {
    e.preventDefault();
  
    if (newPassword !== confirmPassword) {
      dispatch(setMessage({ message: 'Passwords do not match', type: 'error' }));
      return;
    }
  
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.put(
        'https://localhost:7023/User/profile/update',
        {
          name,
          email,
          newPassword,
          confirmPassword,
        },
        {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': token,
          },
        }
      );
  
      if (response.status === 200) {
        dispatch(clearMessage());
        console.log('Profile updated successfully');
      }
    } catch (error) {
      console.error('Profile update failed:', error);
      dispatch(setMessage({ message: 'Profile update failed', type: 'error' }));
    }
  };

  return (
    <div className="container mx-auto mt-5">
      <h1 className="text-2xl font-bold mb-4">Profile</h1>
      {feedback.message && (
        <div className={`text-${feedback.type}-500 mb-4`}>{feedback.message}</div>
      )}
      <form onSubmit={handleUpdate}>
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
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="newPassword">
            New Password
          </label>
          <input
            type="password"
            id="newPassword"
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            placeholder="New Password"
            value={newPassword}
            onChange={(e) => setNewPassword(e.target.value)}
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
            Update
          </button>
        </div>
      </form>
    </div>
  );
}

export default Profile;