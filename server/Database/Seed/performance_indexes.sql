-- Performance Optimization Database Indexes
-- Add these indexes to improve query performance

-- Index for Purchase queries by GiftId and Status (most frequent query)
CREATE INDEX IX_Purchases_GiftId_Status ON Purchases (GiftId, Status);

-- Index for Purchase queries by Status and GiftId (alternative for some queries)
CREATE INDEX IX_Purchases_Status_GiftId ON Purchases (Status, GiftId);

-- Index for LotteryResults by GiftId (frequent lookup)
CREATE INDEX IX_LotteryResults_GiftId ON LotteryResults (GiftId);

-- Composite index for Purchase queries including Quantity for SUM operations
CREATE INDEX IX_Purchases_GiftId_Status_Quantity ON Purchases (GiftId, Status) INCLUDE (Quantity);

-- Index for User queries by ID (if not already exists as primary key)
-- This should already exist as PK, but ensure it's optimized
-- CREATE INDEX IX_Users_Id ON Users (Id);

-- Index for Gift queries by Category (if category filtering is frequent)
CREATE INDEX IX_Gifts_CategoryId ON Gifts (CategoryId);

-- Index for Gift lottery status queries
CREATE INDEX IX_Gifts_IsLotteryCompleted ON Gifts (IsLotteryCompleted);

-- Composite index for efficient gift listing with all required data
CREATE INDEX IX_Gifts_IsLotteryCompleted_CategoryId ON Gifts (IsLotteryCompleted, CategoryId);
