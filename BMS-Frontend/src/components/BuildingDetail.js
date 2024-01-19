// src/components/BuildingDetail.js
import React, { useState } from 'react';
import { useDispatch } from 'react-redux';
import { updateBuilding, deleteBuilding } from '../redux/building';

function BuildingDetail({ building }) {
  const dispatch = useDispatch();
  const [name, setName] = useState(building.name);

  const handleUpdate = () => {
    dispatch(updateBuilding(building.id, name));
  };

  const handleDelete = () => {
    dispatch(deleteBuilding(building.id));
  };

  return (
    <div>
      <input type="text" value={name} onChange={(e) => setName(e.target.value)} />
      <button onClick={handleUpdate}>Update</button>
      <button onClick={handleDelete}>Delete</button>
    </div>
  );
}

export default BuildingDetail;