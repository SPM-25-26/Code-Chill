import { Navigate } from 'react-router-dom';

/**
 * ProtectedRoute is a wrapper component that restricts access to its child components.
 * It checks if a valid authentication token exists in localStorage.
 * If no token is found, it redirects the user to the login page.
 * Otherwise, it renders the child components normally.
 * @param {*} children - The components to render if the user is authenticated
 * @returns {JSX.Element} - Either the children or a redirect to login
 */
function ProtectedRoute({ children }) {
  const token = localStorage.getItem('token');

  if (!token) {
    return <Navigate to="/" replace />;
  }

  return children;
}

export default ProtectedRoute;
