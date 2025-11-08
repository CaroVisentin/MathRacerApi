-- Difficulty
INSERT INTO Difficulty (Name)
VALUES
('Fácil'),
('Medio'),
('Difícil');

-- EnergyConfiguration
INSERT INTO EnergyConfiguration (Price, MaxAmount)
VALUES (2500, 3);

-- ResultType
INSERT INTO ResultType (Name, Description)
VALUES 
('MAYOR', 'El valor de Y debe ser el MAYOR'),
('MENOR', 'El valor de Y debe ser el MENOR');

-- World
INSERT INTO World (Name, OptionsCount, TimePerEquation, OptionRangeMin, OptionRangeMax, NumberRangeMin, NumberRangeMax, DifficultyId)
VALUES 
('Mundo 1', 2, 10, -10, 10, -10, 10, 1),
('Mundo 2', 2, 10, -10, 10, -10, 10, 1),
('Mundo 3', 2, 10, -10, 10, -10, 10, 2),
('Mundo 4', 3, 10, -10, 10, -10, 10, 2),
('Mundo 5', 3, 10, -10, 10, -10, 10, 2);

-- Level (Primero necesitamos insertar los niveles para poder referenciarlos)
INSERT INTO Level (WorldId, Number, TermsCount, VariablesCount, ResultTypeId)
VALUES 
(1, 1, 2, 1, 1),
(1, 2, 2, 1, 2),
(1, 3, 2, 1, 1),
(1, 4, 2, 1, 2),
(1, 5, 2, 1, 1);

-- Operation
INSERT INTO Operation (Sign, Description)
VALUES 
('+', 'Suma'),
('-', 'Resta'),
('*', 'Multiplicación'),
('/', 'División');

-- Player (Ahora con FirebaseUid y referenciando al primer nivel)
INSERT INTO Player (Name, Email, Uid, Coins, LastLevelId, Points, Deleted)
VALUES 
('Lucas', 'lucas@email.com', 'firebase_uid_1', 0, (SELECT TOP 1 Id FROM Level WHERE WorldId = 1 AND Number = 1), 0, 0),
('Mariel', 'mariel@email.com', 'firebase_uid_2', 0, (SELECT TOP 1 Id FROM Level WHERE WorldId = 1 AND Number = 1), 0, 0);

-- Energy (Ahora referenciando a los jugadores creados)
INSERT INTO Energy (PlayerId, Amount, LastConsumptionDate)
SELECT Id, 3, GETDATE()
FROM Player
WHERE Name IN ('Lucas', 'Mariel');

-- ProductType
INSERT INTO ProductType (Name)
VALUES 
('Auto'),
('Personaje'),
('Fondo');

-- Rarity
INSERT INTO Rarity (Rarity, Color)
VALUES 
('Común', '#FFFFFF'),
('Poco Común', '#1EFF00'),
('Raro', '#007BFF'),
('Épico', '#A335EE'),
('Legendario', '#FFA500');

-- Product
INSERT INTO Product (Name, Description, Price, ProductTypeId, RarityId)
VALUES 
('Auto Rojo', 'Auto de color rojo increíble', 5000, 1, 1),
('Personaje default', 'Un personaje común', 5000, 2, 1),
('Fondo de ciudad', 'El fondo que representa a una ciudad', 5000, 3, 1);

-- PlayerProduct (Usando subconsultas para obtener los IDs correctos)
INSERT INTO PlayerProduct (PlayerId, ProductId, IsActive)
SELECT p.Id, pr.Id, 1
FROM Player p
CROSS JOIN Product pr
WHERE p.Name IN ('Lucas', 'Mariel');

-- RequestStatus
INSERT INTO RequestStatus (Name, Description)
VALUES 
('Pendiente', 'Solicitud pendiente de amistad'),
('Aceptada', 'Solicitud de amistad aceptada'),
('Rechazada', 'Solicitud de amistad rechazada');

-- WorldOperation
INSERT INTO WorldOperation (WorldId, OperationId)
VALUES 
(1, 1),
(1, 2),
(2, 3),
(2, 4),
(3, 1),
(3, 2),
(3, 3),
(3, 4),
(4, 1),
(4, 3),
(5, 2),
(5, 4);

-- CoinPackage
INSERT INTO CoinPackage (CoinAmount, Price, Description)
VALUES (1000, 10000, 'Paquete de 1000 monedas');

-- PaymentMethod
INSERT INTO PaymentMethod (Name, Description, Installments)
VALUES ('Mercado Pago', 'Pago a través de código QR', 1);

-- Purchase (Usando subconsulta para obtener el ID del jugador)
INSERT INTO Purchase (PlayerId, Date, TotalAmount, PaymentMethodId, CoinPackageId)
SELECT Id, GETDATE(), 10000, 1, 1
FROM Player
WHERE Name = 'Lucas';

-- Wildcard
INSERT INTO Wildcard (Name, Description, Price)
VALUES 
('Matafuego', 'Permite eliminar una opción incorrecta', 1000.00),
('Cambio de rumbo', 'Permite cambiar la ecuación', 1000.00),
('Nitro', 'La próxima ecuación contará como 2 ecuaciones correctas si se responde correctamente', 1000.00);

-- PlayerWildcard (Usando subconsulta para obtener los IDs de jugadores)
INSERT INTO PlayerWildcard (PlayerId, WildcardId, Quantity)
SELECT p.Id, w.Id, 1
FROM Player p
CROSS JOIN Wildcard w
WHERE p.Name IN ('Lucas', 'Mariel') AND w.Name = 'Matafuego';

-- Friendship (Usando subconsultas para obtener los IDs de jugadores)
INSERT INTO Friendship (PlayerId1, PlayerId2, RequestStatusId)
SELECT p1.Id, p2.Id, (SELECT TOP 1 Id FROM RequestStatus WHERE Name = 'Aceptada')
FROM Player p1
CROSS JOIN Player p2
WHERE p1.Name = 'Lucas' AND p2.Name = 'Mariel';