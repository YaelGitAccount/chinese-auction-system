import api from './axios.js';

// Get all available gifts (not lottery completed)
export const getAvailableGifts = async () => {
  const response = await api.get('/Gift/available');
  return response.data;
};

// Get all gifts (for admin)
export const getAllGifts = async () => {
  const response = await api.get('/Gift');
  return response.data;
};

// Get gift by ID
export const getGiftById = async (id) => {
  const response = await api.get(`/Gift/${id}`);
  return response.data;
};

// Create new gift
export const createGift = async (giftData) => {
  const response = await api.post('/Gift', giftData);
  return response.data;
};

// Update gift
export const updateGift = async (id, giftData) => {
  const response = await api.put(`/Gift/${id}`, giftData);
  return response.data;
};

// Delete gift
export const deleteGift = async (id) => {
  const response = await api.delete(`/Gift/${id}`);
  return response.data;
};

// Add gift to cart
export const addGiftToCart = async (giftId) => {
  const response = await api.post(`/Purchase/cart/${giftId}`);
  return response.data;
};
