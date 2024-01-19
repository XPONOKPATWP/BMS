// src/redux/store.js
import { configureStore } from '@reduxjs/toolkit';
import authReducer from './auth';
import feedbackReducer from './feedback';
import buildingsReducer from './building';
import roomsReducer from './rooms';
import hubsReducer from './hubs';

const store = configureStore({
  reducer: {
    auth: authReducer,
    feedback: feedbackReducer,
    buildings: buildingsReducer,
    rooms: roomsReducer,
    hubs: hubsReducer,
  },
});

export default store;
