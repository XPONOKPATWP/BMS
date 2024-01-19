// src/components/RoomDetail.js
import React, { useState } from 'react';
import { useDispatch } from 'react-redux';
import { updateRoom, deleteRoom } from '../redux/room';

function RoomDetail({ room }) {
  const dispatch = useDispatch();
  const [name, setName] = useState(room.name);

  const handleUpdate = () => {
    dispatch(updateRoom(room.id, name));
  };

  const handleDelete = () => {
    dispatch(deleteRoom(room.id));
  };

  return (
    <div>
      <input type="text" value={name} onChange={(e) => setName(e.target.value)} />
      <button onClick={handleUpdate}>Update</button>
      <button onClick={handleDelete}>Delete</button>
    </div>
  );
}

export default RoomDetail;