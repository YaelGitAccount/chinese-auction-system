import api from "./axios";

export const getAllLotteries = async () => {
  const response = await api.get("/Lottery");
  return response.data;
};

export const getLotteryResults = async () => {
  const response = await api.get("/LotteryResult");
  return response.data;
};

// Get all lottery results (accessible to everyone)
export const getPublicLotteryResults = async () => {
  try {
    const response = await api.get('/Lottery/public-results');
    return response.data;
  } catch (error) {
    console.error('Error fetching public lottery results:', error);
    throw error;
  }
};

// Get lottery results (for managers)
export const getManagerLotteryResults = async () => {
  try {
    const response = await api.get('/Lottery/results');
    return response.data;
  } catch (error) {
    console.error('Error fetching lottery results:', error);
    throw error;
  }
};

// Get public lottery summary (accessible to everyone)
export const getPublicLotterySummary = async () => {
  try {
    const response = await api.get('/Lottery/public-summary');
    return response.data;
  } catch (error) {
    console.error('Error fetching public lottery summary:', error);
    throw error;
  }
};

// Get lottery summary (for managers)
export const getLotterySummary = async () => {
  try {
    const response = await api.get('/Lottery/summary');
    return response.data;
  } catch (error) {
    console.error('Error fetching lottery summary:', error);
    throw error;
  }
};

// Check if lottery is completed
export const isLotteryCompleted = async () => {
  try {
    const response = await api.get('/Lottery/is-completed');
    return response.data.isCompleted;
  } catch (error) {
    console.error('Error checking lottery completion:', error);
    throw error;
  }
};

// Draw lottery for specific gift (for managers)
export const drawLottery = async (giftId) => {
  try {
    const response = await api.post(`/Lottery/draw/${giftId}`);
    return response.data;
  } catch (error) {
    console.error('Error drawing lottery:', error);
    throw error;
  }
};

// Get gifts awaiting lottery (for managers)
export const getGiftsAwaitingLottery = async () => {
  try {
    const response = await api.get('/Lottery/pending-gifts');
    return response.data;
  } catch (error) {
    console.error('Error fetching gifts awaiting lottery:', error);
    throw error;
  }
};

export const createLottery = async (lotteryData) => {
  const response = await api.post("/Lottery", lotteryData);
  return response.data;
};

export const runLottery = async (id) => {
  const response = await api.post(`/Lottery/${id}/run`);
  return response.data;
};
