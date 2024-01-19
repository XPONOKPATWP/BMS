// src/redux/buildings.js
import axios from 'axios';
import { setMessage } from './feedback';

const initialState = [];

export default function buildingsReducer(state = initialState, action) {
  switch (action.type) {
    case 'BUILDINGS_LOADED':
      return action.payload;
    case 'BUILDING_CREATED':
      return [...state, action.payload];
    case 'BUILDING_UPDATED':
      return state.map(building => building.id === action.payload.id ? action.payload : building);
    case 'BUILDING_DELETED':
      return state.filter(building => building.id !== action.payload);
    default:
      return state;   
  }
}

export function listBuildings() {
  return async (dispatch) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.get('https://localhost:7023/Building/GetBuildings', {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token,
        },
      });
      
      if (response.status === 200) {
        dispatch({ type: 'BUILDINGS_LOADED', payload: response.data });
      }
    } catch (error) {
      console.error('Loading buildings failed:', error);
      dispatch(setMessage({ message: 'Loading buildings failed', type: 'error' }));
    }
  };
}

export function createBuilding(name) {
  return async (dispatch) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.post('https://localhost:7023/Building', {
        name
      }, {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token,
        },
      });

      if (response.status === 200) {
        dispatch({ type: 'BUILDING_CREATED', payload: response.data });
        dispatch(listBuildings()); // Update the list of buildings
      }
    } catch (error) {
      console.error('Creating building failed:', error);
      dispatch(setMessage({ message: 'Creating building failed', type: 'error' }));
    }
  };
}

export function updateBuilding(id, name) {
  return async (dispatch) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.put(`https://localhost:7023/Building/${id}`, {
        name
      }, {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token,
        },
      });

      if (response.status === 200) {
        dispatch({ type: 'BUILDING_UPDATED', payload: response.data });
        dispatch(listBuildings()); // Update the list of buildings
      }
    } catch (error) {
      console.error('Updating building failed:', error);
      dispatch(setMessage({ message: 'Updating building failed', type: 'error' }));
    }
  };
}

export function deleteBuilding(id) {
  return async (dispatch) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.delete(`https://localhost:7023/Building/${id}`, {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token,
        },
      });

      if (response.status === 200) {
        dispatch({ type: 'BUILDING_DELETED', payload: id });
        dispatch(listBuildings()); // Update the list of buildings
      }
    } catch (error) {
      console.error('Deleting building failed:', error);
      dispatch(setMessage({ message: 'Deleting building failed', type: 'error' }));
    }
  };
}