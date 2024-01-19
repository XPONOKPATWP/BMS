import axios from 'axios';
import { setMessage } from './feedback';

const initialState = [];

export default function hubsReducer(state = initialState, action) {
  switch (action.type) {
    case 'HUBS_LOADED':
      return action.payload;
    case 'HUB_DISCOVERED':
      return [...state, action.payload];
    case 'HUB_REDISCOVERED':
      return state.map(hub => hub.hubSerialNumber === action.payload.hubSerialNumber ? action.payload : hub);
    case 'HUB_DELETED':
      return state.filter(hub => hub.hubSerialNumber !== action.payload);
    default:
      return state;
  }
}

export function initiateDiscovery(hubData) {
  return async (dispatch) => {
    try {
      const token = localStorage.getItem('authToken');
      const response = await axios.post('https://localhost:7023/Hub/initiate-discovery', hubData, {
        headers: {
          'Content-Type': 'application/json',
          'Authorization': token,
        },
      });

      if (response.status === 200) {
        dispatch({ type: 'HUB_DISCOVERED', payload: response.data });
        // Dispatch action to load hubs here
      }
    } catch (error) {
      console.error('Discovering hub failed:', error);
      dispatch(setMessage({ message: 'Discovering hub failed', type: 'error' }));
    }
  };
}

export function rediscoverHub(hubSerialNumber) {
    return async (dispatch) => {
      try {
        const token = localStorage.getItem('authToken');
        const response = await axios.post(`https://localhost:7023/Hub/rediscover-hub/${hubSerialNumber}`, {}, {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': token,
          },
        });
  
        if (response.status === 200) {
          dispatch({ type: 'HUB_REDISCOVERED', payload: response.data });
          // Dispatch action to load hubs here
        }
      } catch (error) {
        console.error('Rediscovering hub failed:', error);
        dispatch(setMessage({ message: 'Rediscovering hub failed', type: 'error' }));
      }
    };
}  

export function deleteHub(hubSerialNumber) {
    return async (dispatch) => {
      try {
        const token = localStorage.getItem('authToken');
        const response = await axios.delete(`https://localhost:7023/Hub/delete-hub/${hubSerialNumber}`,{
          headers: {
            'Content-Type': 'application/json',
            'Authorization': token,
          },
        });
  
        if (response.status === 200) {
          dispatch({ type: 'HUB_DELETED', payload: hubSerialNumber });
          // Dispatch action to load hubs here
        }
      } catch (error) {
        console.error('Deleting hub failed:', error);
        dispatch(setMessage({ message: 'Deleting hub failed', type: 'error' }));
      }
    };
  }