import { useDispatch } from 'react-redux';
// import your signOut action
import { logout } from '../redux/auth';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';

function SideMenu() {
  const dispatch = useDispatch();

  const navigate = useNavigate();

  const handleSignOut = () => {
    dispatch(logout());
    navigate('/login'); // Redirects the user to the login page
  };

  return (
    <div className="fixed h-full bg-gray-200 w-64 overflow-auto">
      <ul className="space-y-2 p-4">
        <li>
          <Link to="/profile" className="block py-2 px-4 rounded text-blue-500 hover:bg-blue-500 hover:text-white transition duration-200">
            Profile
          </Link>
        </li>
        <li>
          <Link to="/buildings" className="block py-2 px-4 rounded text-blue-500 hover:bg-blue-500 hover:text-white transition duration-200">
            Buildings
          </Link>
        </li>
        <li>
          <Link to="/rooms" className="block py-2 px-4 rounded text-blue-500 hover:bg-blue-500 hover:text-white transition duration-200">
            Rooms
          </Link>
        </li>
        <li>
          <Link to="/hubs" className="block py-2 px-4 rounded text-blue-500 hover:bg-blue-500 hover:text-white transition duration-200">
            Hubs
          </Link>
        </li>
        <li>
          <button onClick={handleSignOut} className="w-full text-left block py-2 px-4 rounded text-blue-500 hover:bg-blue-500 hover:text-white transition duration-200">
            Sign out
          </button>
        </li>
      </ul>
    </div>
  );
}

export default SideMenu;