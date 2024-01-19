// src/components/Buildings.js
import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { listBuildings, createBuilding, deleteBuilding, updateBuilding } from '../redux/building';
import Select from 'react-select';
import { useNavigate } from 'react-router-dom';

function Buildings() {
  const dispatch = useDispatch();
  const buildings = useSelector((state) => state.buildings) || [];
  const [selectedBuildingId, setSelectedBuildingId] = useState(null);
  const [newBuildingName, setNewBuildingName] = useState('');

  const navigate = useNavigate();
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);

  useEffect(() => {
    if (!isAuthenticated) {
        navigate('/login');
    }
    dispatch(listBuildings());
  }, [isAuthenticated, navigate, dispatch]);

  const handleSelect = (selectedOption) => {
    setSelectedBuildingId(selectedOption.value);
  };

  const handleCreateOrUpdate = () => {
    if (selectedBuildingId !== '' && newBuildingName.trim() !== '') {
      dispatch(updateBuilding(selectedBuildingId, newBuildingName)).then(() => {
        setNewBuildingName(''); // Reset the new building name field
        setSelectedBuildingId(null); // Reset the selected building to refresh the component
        dispatch(listBuildings());
      });
    } else if (newBuildingName.trim() !== '') {
      dispatch(createBuilding(newBuildingName)).then(() => {
        setNewBuildingName(''); // Reset the new building name field
        setSelectedBuildingId(null); // Reset the selected building to refresh the component
        dispatch(listBuildings());
      });
    } else {
      return;
    }
  };

  const handleDelete = () => {
    if (selectedBuildingId) {
      dispatch(deleteBuilding(selectedBuildingId)).then(() => {
        setSelectedBuildingId(null); // Reset the selected building to refresh the component
        dispatch(listBuildings());
      });
    }
  };

  

  const selectedBuilding = selectedBuildingId
    ? buildings.find((building) => building.id === Number(selectedBuildingId))
    : null;

    const buildingOptions = [{ value: '', label: 'Create new building...' }, ...buildings.map((building) => ({
        value: building.id,
        label: building.name,
      }))];

  return (
  <div className="container mx-auto mt-5">
    <h1 className="text-2xl font-bold mb-4">Buildings</h1>
    <form>
      <div className="mb-4">
        <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="building">
          Building
        </label>
        <Select
          id="building"
          className="basic-single"
          classNamePrefix="select"
          defaultValue={selectedBuildingId || ''}
          isClearable={false}
          isSearchable={true}
          name="building"
          options={buildingOptions}
          onChange={handleSelect}
        />
      </div>
      <div className="mb-4">
        <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="newBuildingName">
          New Building Name
        </label>
        <input
          type="text"
          id="newBuildingName"
          className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
          placeholder="New building name"
          value={newBuildingName}
          onChange={(e) => setNewBuildingName(e.target.value)}
        />
      </div>
      <div className="flex items-center justify-start mt-4">
        <button
          disabled={newBuildingName.trim() === ''}
          onClick={handleCreateOrUpdate}
          className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline mr-4"
        >
          {selectedBuilding ? 'Update' : 'Create'}
        </button>
        {selectedBuilding && (
          <button
            onClick={handleDelete}
            className="bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline"
          >
            Delete
          </button>
        )}
      </div>
    </form>
  </div>
);
}


export default Buildings;