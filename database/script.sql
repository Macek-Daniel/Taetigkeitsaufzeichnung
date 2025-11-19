IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Taetigkeitsaufzeichnung')
BEGIN
    CREATE DATABASE Taetigkeitsaufzeichnung;
END
GO
USE Taetigkeitsaufzeichnung;
GO

CREATE TABLE Lehrer (
    LehrerID INT PRIMARY KEY IDENTITY(1,1),
    Vorname NVARCHAR(100) NOT NULL,
    Nachname NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE Schuljahre (
    SchuljahrID INT PRIMARY KEY IDENTITY(1,1),
    Bezeichnung NVARCHAR(20) NOT NULL UNIQUE,
    Startdatum DATE NOT NULL,
    Enddatum DATE NOT NULL
);

CREATE TABLE Projekte (
    ProjektID INT PRIMARY KEY IDENTITY(1,1),
    Projektname NVARCHAR(200) NOT NULL UNIQUE
);

CREATE TABLE LehrerSchuljahrSollstunden (
    LehrerID INT NOT NULL,
    SchuljahrID INT NOT NULL,
    Sollstunden DECIMAL(10, 2) NOT NULL,
    PRIMARY KEY (LehrerID, SchuljahrID),
    FOREIGN KEY (LehrerID) REFERENCES Lehrer(LehrerID) ON DELETE CASCADE,
    FOREIGN KEY (SchuljahrID) REFERENCES Schuljahre(SchuljahrID) ON DELETE CASCADE
);

CREATE TABLE Taetigkeiten (
    TaetigkeitID INT PRIMARY KEY IDENTITY(1,1),
    Datum DATE NOT NULL,
    Beschreibung NVARCHAR(MAX) NOT NULL,
    DauerStunden DECIMAL(10, 2) NOT NULL,
    LehrerID INT NOT NULL,
    ProjektID INT NOT NULL,
    FOREIGN KEY (LehrerID) REFERENCES Lehrer(LehrerID),
    FOREIGN KEY (ProjektID) REFERENCES Projekte(ProjektID)
);


INSERT INTO Lehrer (Vorname, Nachname) VALUES
('Alexander', 'Greil'),
('Max', 'Mustermann'),
('Martina', 'Musterfrau'),
('Brandy', '(Nachname unklar)');

INSERT INTO Schuljahre (Bezeichnung, Startdatum, Enddatum) VALUES
('2024/25', '2024-09-01', '2025-08-31'),
('2025/26', '2025-09-01', '2026-08-31');

INSERT INTO Projekte (Projektname) VALUES
('Diplomarbeit Betreuung'),
('Werkstätten-Modernisierung'),
('Tag der offenen Tür'),
('IT-Infrastruktur Wartung');

INSERT INTO LehrerSchuljahrSollstunden (LehrerID, SchuljahrID, Sollstunden) VALUES
(1, 2, 420.00),
(4, 2, 200.00);

INSERT INTO Taetigkeiten (Datum, Beschreibung, DauerStunden, LehrerID, ProjektID) VALUES
('2025-09-15', 'Kick-off Meeting für Diplomarbeiten', 8.00, 1, 1),
('2025-09-22', 'Planung der neuen CNC-Fräse', 15.00, 1, 2),
('2025-09-28', 'Vorbereitung für den Tag der offenen Tür', 6.00, 1, 3),
('2025-10-01', 'Server-Wartung und Updates', 40.00, 1, 4);

INSERT INTO Taetigkeiten (Datum, Beschreibung, DauerStunden, LehrerID, ProjektID) VALUES
('2025-09-18', 'Konzept-Brainstorming TdoT', 12.00, 4, 3),
('2025-09-25', 'Rücksprache mit Schülern', 8.00, 4, 1);
