import React, { useState } from 'react';
import { useDispatch } from 'react-redux';
import { rediscoverHub, deleteHub } from '../redux/hubs';

function HubDetail({ hub }) {
  const dispatch = useDispatch();
  const [hubSerialNumber, setHubSerialNumber] = useState(hub.hubSerialNumber);

  const handleRediscover = () => {
    dispatch(rediscoverHub(hubSerialNumber));
  };

  const handleDelete = () => {
    dispatch(deleteHub(hubSerialNumber));
    setHubSerialNumber('');
  };

  return (
    <div>
      <input type="text" value={hubSerialNumber} onChange={(e) => setHubSerialNumber(e.target.value)} />
      <button onClick={handleRediscover}>Rediscover</button>
      <button onClick={handleDelete}>Delete</button>
    </div>
  );
}

export default HubDetail;