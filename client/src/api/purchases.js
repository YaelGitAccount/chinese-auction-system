import api from './axios.js';

// Get user's cart
export const getCart = async () => {
  const response = await api.get('/Purchase/cart');
  return response.data;
};

// Add gift to cart
export const addToCart = async (giftId) => {
  const response = await api.post(`/Purchase/cart/${giftId}`);
  return response.data;
};

// Remove from cart
export const removeFromCart = async (purchaseId) => {
  const response = await api.delete(`/Purchase/${purchaseId}`);
  return response.data;
};

// Purchase cart (checkout)
export const purchaseCart = async () => {
  const response = await api.post('/Purchase/checkout');
  return response.data;
};

// Get user's purchases
export const getUserPurchases = async () => {
  const response = await api.get('/Purchase/user');
  return response.data;
};

// Get all purchases (admin)
export const getAllPurchases = async () => {
  const response = await api.get('/Purchase');
  return response.data;
};
