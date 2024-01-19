// src/redux/rooms.js
import axios from 'axios';
import { setMessage } from './feedback';

const initialState = [];

export default function roomsReducer(state = initialState, action) {
  switch (action.type) {
    case 'ROOMS_LOADED':
      return action.payload.map(room => ({
        ...room,
        hubs: room.hubs.map(hub => ({
          ...hub,
          devices: hub.devices
        }))
      }));
    case 'ROOM_CREATED':
      return [...state, action.payload];
    case 'ROOM_UPDATED':
      return state.map(room => room.id === action.payload.id ? action.payload : room);
    case 'ROOM_DELETED':
      return state.filter(room => room.id !== action.payload);
    case 'ROOM_FETCHED':
      return state.map(room => room.id === action.payload.id ? action.payload : room);
    default:
      return state;
  }
}

export function listRooms() {
  return async (dispatch) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.get('https://localhost:7023/Room/GetRooms', {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token,
        },
      });

      if (response.status === 200) {
        dispatch({ type: 'ROOMS_LOADED', payload: response.data });
      }
    } catch (error) {
      console.error('Loading rooms failed:', error);
      dispatch(setMessage({ message: 'Loading rooms failed', type: 'error' }));
    }
  };
}

export function createRoom(name, buildingId) {
  return async (dispatch) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.post('https://localhost:7023/Room', {
        name,
        buildingId
      }, {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token,
        },
      });

      if (response.status === 200) {
        dispatch({ type: 'ROOM_CREATED', payload: response.data });
        dispatch(listRooms());
      }
    } catch (error) {
      console.error('Creating room failed:', error);
      dispatch(setMessage({ message: 'Creating room failed', type: 'error' }));
    }
  };
}

export function updateRoom(id, name, buildingId) {
  return async (dispatch) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.put(`https://localhost:7023/Room/${id}`, {
        name,
        buildingId
      }, {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token,
        },
      });

      if (response.status === 200) {
        dispatch({ type: 'ROOM_UPDATED', payload: response.data });
        dispatch(listRooms());
      }
    } catch (error) {
      console.error('Updating room failed:', error);
      dispatch(setMessage({ message: 'Updating room failed', type: 'error' }));
    }
  };
}

export function deleteRoom(id) {
  return async (dispatch) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.delete(`https://localhost:7023/Room/${id}`, {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token,
        },
      });

      if (response.status === 200) {
        dispatch({ type: 'ROOM_DELETED', payload: id });
        dispatch(listRooms());
      }
    } catch (error) {
      console.error('Deleting room failed:', error);
      dispatch(setMessage({ message: 'Deleting room failed', type: 'error' }));
    }
  };
}

export function fetchRoom(id) {
    return async (dispatch) => {
      try {
        const token = localStorage.getItem('authToken');
        const response = await axios.get(`https://localhost:7023/Room/${id}`, {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': token,
          },
        });
  
        if (response.status === 200) {
          dispatch({ type: 'ROOM_FETCHED', payload: response.data });
        }
      } catch (error) {
        console.error('Fetching room failed:', error);
        dispatch(setMessage({ message: 'Fetching room failed', type: 'error' }));
      }
    };
  }
