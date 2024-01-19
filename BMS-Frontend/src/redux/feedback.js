// src/redux/feedback.js
import { createSlice } from '@reduxjs/toolkit';

const feedbackSlice = createSlice({
  name: 'feedback',
  initialState: {
    message: '',
    type: 'success', // success, error, info, etc.
  },
  reducers: {
    setMessage: (state, action) => {
      state.message = action.payload.message;
      state.type = action.payload.type;
    },
    clearMessage: (state) => {
      state.message = '';
      state.type = 'success';
    },
  },
});

export const { setMessage, clearMessage } = feedbackSlice.actions;
export default feedbackSlice.reducer;
