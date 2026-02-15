-- ==========================================
-- Proceduri stocate pentru aplicația Restaurant Management
-- ==========================================

-- ==========================================
-- 1. Procedură pentru obținerea preparatelor disponibile cu categoria lor
-- ==========================================
CREATE OR ALTER PROCEDURE sp_GetAvailableDishesWithCategory
    AS
BEGIN
    SET NOCOUNT ON;

SELECT
    d.DishID,
    d.Name AS DishName,
    d.Price,
    d.PortionQuantityGrams,
    c.Name AS CategoryName
FROM Dishes d
         INNER JOIN Categories c ON d.CategoryID = c.CategoryID
WHERE d.IsAvailable = 1
ORDER BY c.Name, d.Name;
END
GO

-- ==========================================
-- 2. Procedură pentru obținerea clienților frecvenți
-- ==========================================
CREATE OR ALTER PROCEDURE sp_GetFrequentClients
    @DaysPeriod INT = 30,
    @MinOrderCount INT = 3
    AS
BEGIN
    SET NOCOUNT ON;

SELECT
    u.UserID,
    u.FirstName,
    u.LastName,
    u.Email,
    u.Phone,
    COUNT(o.OrderID) AS OrdersCount,
    SUM(o.TotalCost) AS TotalSpent,
    AVG(o.TotalCost) AS AverageOrderValue
FROM Users u
         INNER JOIN Orders o ON u.UserID = o.UserID
WHERE o.OrderDate >= DATEADD(DAY, -@DaysPeriod, GETDATE())
  AND o.Status != 'anulata'
      AND u.UserType = 'client'
GROUP BY u.UserID, u.FirstName, u.LastName, u.Email, u.Phone
HAVING COUNT(o.OrderID) >= @MinOrderCount
ORDER BY OrdersCount DESC, TotalSpent DESC;
END
GO

-- ==========================================
-- 3. Procedură pentru adăugarea unui preparat nou
-- ==========================================
CREATE OR ALTER PROCEDURE sp_AddNewDish
    @Name NVARCHAR(100),
    @Description NVARCHAR(MAX) = NULL,
    @Price DECIMAL(10,2),
    @PortionQuantityGrams INT,
    @TotalQuantityGrams INT,
    @CategoryID INT,
    @IsAvailable BIT = 1
    AS
BEGIN
    SET NOCOUNT ON;

INSERT INTO Dishes (Name, Description, Price, PortionQuantityGrams, TotalQuantityGrams, CategoryID, IsAvailable)
VALUES (@Name, @Description, @Price, @PortionQuantityGrams, @TotalQuantityGrams, @CategoryID, @IsAvailable);

-- Returnăm ID-ul noului preparat pentru confirmare
SELECT SCOPE_IDENTITY() AS NewDishID;
END
GO

-- ==========================================
-- 4. Procedură pentru actualizarea disponibilității unui preparat
-- ==========================================
CREATE OR ALTER PROCEDURE sp_UpdateDishAvailability
    @DishID INT,
    @IsAvailable BIT
    AS
BEGIN
    SET NOCOUNT ON;

UPDATE Dishes
SET IsAvailable = @IsAvailable
WHERE DishID = @DishID;

SELECT
    d.DishID,
    d.Name,
    d.Price,
    d.PortionQuantityGrams,
    d.TotalQuantityGrams,
    d.CategoryID,
    c.Name AS CategoryName,
    d.IsAvailable
FROM Dishes d
         INNER JOIN Categories c ON d.CategoryID = c.CategoryID
WHERE d.DishID = @DishID;
END
GO

-- ==========================================
-- 5. Procedură pentru obținerea detaliilor unei comenzi
-- ==========================================
CREATE OR ALTER PROCEDURE sp_GetOrderDetailsById
    @OrderID INT
    AS
BEGIN
    SET NOCOUNT ON;
    
    -- Informații generale despre comandă
SELECT
    o.OrderID,
    o.OrderCode,
    o.OrderDate,
    o.Status,
    o.EstimatedDeliveryTime,
    o.FoodCost,
    o.DeliveryCost,
    o.DiscountValue,
    o.TotalCost,
    u.FirstName + ' ' + u.LastName AS CustomerName,
    u.Phone AS CustomerPhone,
    u.DeliveryAddress
FROM Orders o
         INNER JOIN Users u ON o.UserID = u.UserID
WHERE o.OrderID = @OrderID;

-- Preparate comandate
SELECT
    'Dish' AS ItemType,
    d.DishID,
    d.Name AS ItemName,
    od.Quantity,
    d.Price AS UnitPrice,
    od.Quantity * d.Price AS TotalPrice
FROM OrderDishes od
         INNER JOIN Dishes d ON od.DishID = d.DishID
WHERE od.OrderID = @OrderID;

-- Meniuri comandate
SELECT
    'Menu' AS ItemType,
    m.MenuID,
    m.Name AS ItemName,
    om.Quantity,
    m.Price AS UnitPrice,
    om.Quantity * m.Price AS TotalPrice
FROM OrderMenus om
         INNER JOIN Menus m ON om.MenuID = m.MenuID
WHERE om.OrderID = @OrderID;
END
GO
