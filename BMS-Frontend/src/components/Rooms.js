import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import Select from 'react-select';
import { listRooms, createRoom, deleteRoom, updateRoom } from '../redux/rooms';
import { listBuildings } from '../redux/building';
import { useNavigate } from 'react-router-dom';

function Rooms() {
  const dispatch = useDispatch();
  const rooms = useSelector((state) => state.rooms) || [];
  const [selectedRoomId, setSelectedRoomId] = useState(null);
  const [newRoomName, setNewRoomName] = useState('');
  const [selectedBuildingId, setSelectedBuildingId] = useState(null);

  const buildings = useSelector((state) => state.buildings) || [];

  const navigate = useNavigate();
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);

  useEffect(() => {
    if (!isAuthenticated) {
        navigate('/login');
    }
    dispatch(listRooms());
    dispatch(listBuildings());
  }, [isAuthenticated, navigate, dispatch]);

  const handleSelect = (selectedOption) => {
    setSelectedRoomId(selectedOption ? selectedOption.value : '');
    if (selectedOption) {
      const selectedRoom = rooms.find(room => room.id === selectedOption.value);
      if (selectedRoom) {
        setSelectedBuildingId(selectedRoom.buildingId);
        setNewRoomName(selectedRoom.name);
      }
    } else {
      setSelectedBuildingId(null);
      setNewRoomName('');
    }
  };

  const handleCreateOrUpdate = () => {
    if (selectedRoomId && newRoomName.trim() !== '' && selectedBuildingId !== null) {
      dispatch(updateRoom(selectedRoomId, newRoomName, selectedBuildingId)).then(() => {
        setNewRoomName(''); // Reset the new room name field
        setSelectedRoomId(null); // Reset the selected room to refresh the component
        dispatch(listRooms());
      });
    } else if (newRoomName.trim() !== '' && selectedBuildingId !== null) {
      dispatch(createRoom(newRoomName, selectedBuildingId)).then(() => {
        setNewRoomName(''); // Reset the new room name field
        setSelectedRoomId(null); // Reset the selected room to refresh the component
        dispatch(listRooms());
      });
    } else {
      return;
    }
  };

  const handleDelete = () => {
    if (selectedRoomId) {
      dispatch(deleteRoom(selectedRoomId));
      setSelectedRoomId(null);
      dispatch(listRooms());
    }
  };

  const roomOptions = [{ value: '', label: 'Create new room...' }, ...rooms.map((room) => ({
    value: room.id,
    label: room.name,
    buildingId: room.buildingId,
  }))];

  const buildingOptions = buildings.map((building) => ({
    value: building.id,
    label: building.name,
  }));

  return (
    <div className="container mx-auto mt-5">
      <h1 className="text-2xl font-bold mb-4">Rooms</h1>
      <form>
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="room">
            Room
          </label>
          <Select
            id="room"
            className="basic-single"
            classNamePrefix="select"
            defaultValue={selectedRoomId || ''}
            isClearable={true}
            isSearchable={true}
            name="room"
            options={roomOptions}
            onChange={handleSelect}
          />
        </div>
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="newRoomName">
            New Room Name
          </label>
          <input
            type="text"
            id="newRoomName"
            className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
            placeholder="New room name"
            value={newRoomName}
            onChange={(e) => setNewRoomName(e.target.value)}
          />
        </div>
        <div className="mb-4">
          <label className="block text-gray-700 text-sm font-bold mb-2" htmlFor="room">
            Building
          </label>
          <Select
            className="basic-single"
            classNamePrefix="select"
            value={buildingOptions.find(option => option.value === selectedBuildingId) || null}
            isClearable={false}
            isSearchable={true}
            name="building"
            options={buildingOptions}
            onChange={(selectedOption) => setSelectedBuildingId(selectedOption ? selectedOption.value : '')}
            isDisabled={selectedRoomId === null}
           />
        </div>
        <div className="flex items-center justify-start mt-4">
          <button
            type="submit"
            disabled={!newRoomName.trim() || !selectedBuildingId} // disable button when newRoomName is blank or no building is selected
            onClick={handleCreateOrUpdate}
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline mr-4"
          >
            {selectedRoomId ? 'Update' : 'Create'}
          </button>
          {selectedRoomId && (
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

export default Rooms;