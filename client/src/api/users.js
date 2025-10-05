import api from './axios.js';

// User registration
export const register = async (userData) => {
  const response = await api.post('/User/register', userData);
  return response.data;
};

// User login
export const login = async (credentials) => {
  const response = await api.post('/User/login', credentials);
  return response.data;
};

// Get all users (admin)
export const getAllUsers = async () => {
  const response = await api.get('/User');
  return response.data;
};

// Get user by ID
export const getUserById = async (id) => {
  const response = await api.get(`/User/${id}`);
  return response.data;
};

// Update user
export const updateUser = async (id, userData) => {
  const response = await api.put(`/User/${id}`, userData);
  return response.data;
};

// Delete user
export const deleteUser = async (id) => {
  const response = await api.delete(`/User/${id}`);
  return response.data;
};
