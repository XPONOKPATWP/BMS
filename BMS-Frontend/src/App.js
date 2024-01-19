import React, { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, useLocation } from 'react-router-dom';
import { Provider } from 'react-redux';
import { Navigate } from 'react-router-dom';
import store from './redux/store';
import Login from './components/Login';
import Register from './components/Register';
import Profile from './components/Profile';
import Buildings from './components/Buildings';
import Rooms from './components/Rooms';
import Hubs from './components/Hubs';
import SideMenu from './components/SideMenu'; // import the SideMenu component

const useDocumentTitle = (title) => {
  useEffect(() => {
    document.title = title;
    return () => {
    };
  }, [title]);
};


function Layout({ children }) {
  useDocumentTitle('Building Management System');
  const location = useLocation();

  return (
    <div className="layout flex">
      {location.pathname !== '/' && location.pathname !== '/login' && location.pathname !== '/register' && <SideMenu />}
      <div className="content flex-grow ml-64 p-4">
        {children}
      </div>
    </div>
  );
}

function App() {
  return (
    <Provider store={store}>
      <Router>
        <Layout>
          <Routes>
            <Route path="/" element={<Navigate to="/login" />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/profile" element={<Profile />} />
            <Route path="/buildings" element={<Buildings />} />
            <Route path="/rooms" element={<Rooms />} />
            <Route path="/hubs" element={<Hubs />} />
          </Routes>
        </Layout>
      </Router>
    </Provider>
  );
}

export default App;