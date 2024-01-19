import React, { useEffect, useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import { listBuildings } from "../redux/building";
import { listRooms } from "../redux/rooms"; // import your room actions
import { initiateDiscovery, rediscoverHub, deleteHub } from "../redux/hubs"; // import your hub actions
import Select from "react-select";
import { useNavigate } from "react-router-dom";

function Hubs() {
  const dispatch = useDispatch();
  const buildings = useSelector((state) => state.buildings) || [];
  const rooms = useSelector((state) => state.rooms) || []; // reducer to get the rooms
  const [selectedBuildingId, setSelectedBuildingId] = useState(null);
  const [selectedRoomId, setSelectedRoomId] = useState(null);
  const [hubAction, setHubAction] = useState(null);
  const [hubSerialNumber, setHubSerialNumber] = useState("");

  const navigate = useNavigate();
  const isAuthenticated = useSelector((state) => state.auth.isAuthenticated);

  const hubs = useSelector((state) => state.hubs) || [];

  const selectedRoom = rooms.find((room) => room.id === selectedRoomId);
  const hubsOfSelectedRoom = selectedRoom ? selectedRoom.hubs : [];

  const [selectedHub, setSelectedHub] = useState(null);
  const [showDevices, setShowDevices] = useState(false);

  const [deviceReadings, setDeviceReadings] = useState([]);

  useEffect(() => {
    if (!isAuthenticated) {
      navigate("/login");
    }
    dispatch(listBuildings());
    // dispatch action to list rooms here
  }, [isAuthenticated, navigate, dispatch]);

  const handleBuildingSelect = (selectedOption) => {
    setSelectedBuildingId(selectedOption ? selectedOption.value : "");
    if (selectedOption) {
      dispatch(listRooms(selectedOption.value)); // Add this line
    }
  };

  const handleRoomSelect = (selectedOption) => {
    setSelectedRoomId(selectedOption ? selectedOption.value : "");
  };

  const handleHubActionSelect = (selectedOption) => {
    console.log("Selected Hub Option:", selectedOption);
    setSelectedHub(
      hubsOfSelectedRoom.find(
        (hub) => hub.id.toString() === selectedOption.value
      )
    );
    setShowDevices(false); // Reset show devices flag
    if (selectedOption.value === "add") {
      setHubAction("add");
      setHubSerialNumber("");
    } else {
      setHubAction("existing");
      setHubSerialNumber(selectedOption.serial);
    }
  };

  const handleDiscover = () => {
    if (hubSerialNumber.trim() !== "" && selectedRoomId !== null) {
      dispatch(initiateDiscovery({ hubSerialNumber, roomID: selectedRoomId }));
      setHubSerialNumber("");
    }
  };

  const handleRediscover = () => {
    if (hubSerialNumber.trim() !== "") {
      dispatch(rediscoverHub(hubSerialNumber));
    }
  };

  const handleDelete = async () => {
    if (hubSerialNumber && hubSerialNumber.trim() !== "") {
      await dispatch(deleteHub(hubSerialNumber));
      setHubSerialNumber("");
    }
  };

  const fetchDeviceReadings = async (hubId) => {
    try {
      const token = localStorage.getItem("authToken");
      const response = await fetch(
        `https://localhost:7023/Hub/${hubId}/readings`,
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: token,
          },
        }
      );
      if (!response.ok) {
        throw new Error("Network response was not ok");
      }
      const data = await response.json();
      setDeviceReadings(data);
    } catch (error) {
      console.error("There was a problem with the fetch operation:", error);
    }
  };

  const sendCommand = async (deviceId, command) => {
    try {
      console.log(`Sending command ${command} to device ${deviceId}`);
      const token = localStorage.getItem("authToken");
      const response = await fetch(
        `https://localhost:7023/Hub/device-command/${deviceId}`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: token,
          },
          body: JSON.stringify({ command }),
        }
      );

      if (!response.ok) {
        throw new Error("Network response was not ok");
      }
      console.log("Command sent successfully");
    } catch (error) {
      console.error("There was an error sending the command:", error);
    }
  };

  // Add select options for buildings, rooms, and hub actions here

  const buildingOptions = buildings.map((building) => ({
    value: building.id,
    label: building.name,
  }));

  const roomOptions = rooms
    .filter((room) => room.buildingId === selectedBuildingId)
    .map((room) => ({
      value: room.id,
      label: room.name,
    }));

  const hubOptions = [
    { value: "add", label: "Add new hub" },
    ...hubsOfSelectedRoom.map((hub) => ({
      value: hub.id.toString(), // Convert hub.id to a string
      label: hub.name,
      serial: hub.serialNumber,
    })),
  ];
  return (
    <div className="container mx-auto mt-5">
      <h1 className="text-2xl font-bold mb-4">Hubs</h1>
      <form>
        <div className="mb-4">
          <label
            className="block text-gray-700 text-sm font-bold mb-2"
            htmlFor="building"
          >
            Building
          </label>
          <Select
            id="building"
            className="basic-single"
            classNamePrefix="select"
            defaultValue={selectedBuildingId || ""}
            isClearable={false}
            isSearchable={true}
            name="building"
            options={buildingOptions}
            onChange={handleBuildingSelect}
          />
        </div>
        <div className="mb-4">
          <label
            className="block text-gray-700 text-sm font-bold mb-2"
            htmlFor="room"
          >
            Room
          </label>
          <Select
            id="room"
            className="basic-single"
            classNamePrefix="select"
            defaultValue={selectedRoomId || ""}
            isClearable={false}
            isSearchable={true}
            name="room"
            options={roomOptions}
            onChange={handleRoomSelect}
            isDisabled={!selectedBuildingId}
          />
        </div>
        <div className="mb-4">
          <label
            className="block text-gray-700 text-sm font-bold mb-2"
            htmlFor="hubAction"
          >
            Hub Action
          </label>
          <Select
            className="basic-single"
            classNamePrefix="select"
            defaultValue={hubAction || ""}
            isClearable={false}
            isSearchable={true}
            name="hub"
            options={hubOptions}
            onChange={handleHubActionSelect}
            isDisabled={!selectedRoomId}
          />
        </div>
        {hubAction === "add" && (
          <div className="mb-4">
            <label
              className="block text-gray-700 text-sm font-bold mb-2"
              htmlFor="hubSerialNumber"
            >
              Hub Serial Number
            </label>
            <input
              type="text"
              id="hubSerialNumber"
              className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline"
              placeholder="Hub serial number"
              value={hubSerialNumber}
              onChange={(e) => setHubSerialNumber(e.target.value)}
            />
            <br />
            <br />
            <button
              onClick={handleDiscover}
              className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline mr-4"
            >
              Discover
            </button>
          </div>
        )}
        {hubAction === "existing" && (
          <div className="mb-4">
            <button
              onClick={handleRediscover}
              className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline mr-4"
            >
              Rediscover
            </button>
            <button
              onClick={handleDelete}
              className="bg-red-500 hover:bg-red-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline"
            >
              Delete
            </button>
          </div>
        )}
        {selectedHub && (
          <div className="mb-4">
            <button
              onClick={(e) => {
                e.preventDefault();
                setShowDevices(!showDevices);
                if (!showDevices) {
                  fetchDeviceReadings(selectedHub.id);
                }
              }}
              className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline mr-4"
            >
              {showDevices ? "Hide Devices" : "Show Devices"}
            </button>
          </div>
        )}
      </form>

      {showDevices &&
        selectedHub &&
        selectedHub.devices.map((device) => {
          const reading =
            deviceReadings.find((r) => r.deviceId === device.id) || {};

          // Safely parse the Capabilities JSON string
          let capabilities = [];
          try {
            capabilities = device.deviceCapabilities.map((cap) =>
              JSON.parse(cap.capabilityType)
            );

            console.log("Device:", device);
            console.log("Parsed capabilities:", capabilities);
          } catch (error) {
            console.error("Error parsing capabilities:", error);
            // Handle the error appropriately
          }

          return (
            <div key={device.id} className="bg-gray-100 p-4 my-2 rounded">
              <p>
                <strong>Device Name:</strong>{" "}
                {device.name || reading.deviceName}
              </p>
              <p>
                <strong>Status:</strong> {reading.status || "Unknown"}
              </p>

              {/* Latest and Average Readings */}
              {reading.latestReadings &&
                Object.entries(reading.latestReadings).map(([key, value]) => (
                  <p key={`latest-${key}`}>
                    <strong>
                      Latest {key.charAt(0).toUpperCase() + key.slice(1)}:
                    </strong>{" "}
                    {value || "N/A"}
                  </p>
                ))}
              {reading.averageReadings &&
                Object.entries(reading.averageReadings).map(([key, value]) => (
                  <p key={`average-${key}`}>
                    <strong>
                      Average {key.charAt(0).toUpperCase() + key.slice(1)}:
                    </strong>{" "}
                    {value || "N/A"}
                  </p>
                ))}

              {/* Device Capabilities */}
              {capabilities.map((capability, index) => {
                if (
                  capability.Commands &&
                  capability.Commands.some((command) => command !== "read")
                ) {
                  return (
                    <div key={`${device.id}-capability-${index}`}>
                      <p>Commands for {capability.Type}:</p>
                      {capability.Commands.map((command) => (
                        <button
                          key={`${device.id}-command-${command}`}
                          onClick={() => sendCommand(device.id, command)}
                          className="mr-2 mb-2 bg-blue-500 hover:bg-blue-700 text-white font-bold py-1 px-2 rounded"
                        >
                          {command}
                        </button>
                      ))}
                    </div>
                  );
                } else {
                  return null;
                }
              })}
            </div>
          );
        })}
    </div>
  );
}

export default Hubs;
