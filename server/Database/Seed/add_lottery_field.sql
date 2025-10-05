-- SQL Script להוספת שדה IsLotteryCompleted לטבלת Gifts
-- הרץ את הסקריפט הזה ישירות בבסיס הנתונים אם המיגרציה לא עובדת

USE ChineseAuctionDb;
GO

-- בדיקה אם השדה כבר קיים
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('dbo.Gifts') 
    AND name = 'IsLotteryCompleted'
)
BEGIN
    -- הוספת השדה עם ערך ברירת מחדל
    ALTER TABLE dbo.Gifts 
    ADD IsLotteryCompleted bit NOT NULL DEFAULT 0;
    
    PRINT 'השדה IsLotteryCompleted נוסף בהצלחה לטבלת Gifts';
END
ELSE
BEGIN
    PRINT 'השדה IsLotteryCompleted כבר קיים בטבלת Gifts';
END

-- עדכון המתנות שכבר יש להן תוצאת הגרלה
UPDATE g 
SET IsLotteryCompleted = 1
FROM dbo.Gifts g
INNER JOIN dbo.LotteryResults lr ON g.Id = lr.GiftId;

PRINT 'עודכנו מתנות שכבר יש להן תוצאת הגרלה';

-- בדיקה שהכל עבד
SELECT 
    COUNT(*) as TotalGifts,
    SUM(CASE WHEN IsLotteryCompleted = 1 THEN 1 ELSE 0 END) as CompletedLotteries,
    SUM(CASE WHEN IsLotteryCompleted = 0 THEN 1 ELSE 0 END) as AvailableGifts
FROM dbo.Gifts;
