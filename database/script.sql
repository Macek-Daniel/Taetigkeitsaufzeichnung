/*
  Datenbank-Initialisierungsskript (SQL Server)
  - Legt Tabellen, Primär-/Fremdschlüssel und Indizes an
  - Abbild der aktuellen EF-Modelle inkl. Abteilung/Abteilungsvorstand
  - Abbild der aktuellen EF-Modelle mit Azure AD Object ID (NVARCHAR(36)) als LehrerID
  Hinweis: Dieses Skript ist für Microsoft SQL Server (T-SQL) ausgelegt.
*/
DROP DATABASE IF EXISTS [Taetigkeitsaufzeichnung];
CREATE DATABASE [Taetigkeitsaufzeichnung];
GO
USE [Taetigkeitsaufzeichnung];
SET NOCOUNT ON;

/* In korrekter Reihenfolge löschen (abhängige Tabellen zuerst) */
IF OBJECT_ID(N'[dbo].[Abteilungsvorstand]', 'U') IS NOT NULL DROP TABLE [dbo].[Abteilungsvorstand];
IF OBJECT_ID(N'[dbo].[Taetigkeit]', 'U') IS NOT NULL DROP TABLE [dbo].[Taetigkeit];
IF OBJECT_ID(N'[dbo].[LehrerSchuljahrSollstunden]', 'U') IS NOT NULL DROP TABLE [dbo].[LehrerSchuljahrSollstunden];
IF OBJECT_ID(N'[dbo].[Abteilung]', 'U') IS NOT NULL DROP TABLE [dbo].[Abteilung];
IF OBJECT_ID(N'[dbo].[Projekt]', 'U') IS NOT NULL DROP TABLE [dbo].[Projekt];
IF OBJECT_ID(N'[dbo].[Schuljahr]', 'U') IS NOT NULL DROP TABLE [dbo].[Schuljahr];
IF OBJECT_ID(N'[dbo].[Lehrer]', 'U') IS NOT NULL DROP TABLE [dbo].[Lehrer];

/* Basis-Tabellen */
CREATE TABLE [dbo].[Lehrer]
(
    [LehrerID]  NVARCHAR(36)       NOT NULL CONSTRAINT [PK_Lehrer] PRIMARY KEY,
    [Vorname]   NVARCHAR(100)      NOT NULL,
    [Nachname]  NVARCHAR(100)      NOT NULL,
    [Email]     NVARCHAR(256)      NOT NULL,
    [LoginName] NVARCHAR(256)      NULL,
    [CreatedAt] DATETIME2          NOT NULL CONSTRAINT [DF_Lehrer_CreatedAt] DEFAULT(GETUTCDATE()),
    [IsActive]  BIT                NOT NULL CONSTRAINT [DF_Lehrer_IsActive] DEFAULT(1)
);

CREATE TABLE [dbo].[Schuljahr]
(
    [SchuljahrID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Schuljahr] PRIMARY KEY,
    [Bezeichnung] NVARCHAR(20)       NOT NULL,
    [Startdatum]  DATE               NOT NULL,
    [Enddatum]    DATE               NOT NULL
);

/* UNIQUE auf Bezeichnung wie im Modell */
CREATE UNIQUE INDEX [IX_Schuljahr_Bezeichnung]
ON [dbo].[Schuljahr]([Bezeichnung]);

CREATE TABLE [dbo].[Projekt]
(
    [ProjektID]   INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Projekt] PRIMARY KEY,
    [Projektname] NVARCHAR(200)      NOT NULL
);

/* UNIQUE auf Projektname wie im Modell */
CREATE UNIQUE INDEX [IX_Projekt_Projektname]
ON [dbo].[Projekt]([Projektname]);

CREATE TABLE [dbo].[Abteilung]
(
    [AbteilungID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Abteilung] PRIMARY KEY,
    [Name]        NVARCHAR(100)      NOT NULL,
    [Kuerzel]     NVARCHAR(20)       NULL,
    [IsActive]    BIT                NOT NULL CONSTRAINT [DF_Abteilung_IsActive] DEFAULT(1)
);

/* Abhängige Tabellen */
CREATE TABLE [dbo].[Taetigkeit]
(
    [TaetigkeitID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Taetigkeit] PRIMARY KEY,
    [Datum]        DATE               NOT NULL,
    [Beschreibung] NVARCHAR(MAX)      NOT NULL,
    [DauerStunden] DECIMAL(18,2)      NOT NULL CONSTRAINT [DF_Taetigkeit_DauerStunden] DEFAULT(0),
    [LehrerID]     NVARCHAR(36)       NOT NULL,
    [ProjektID]    INT                NOT NULL
);

/* FKs für Taetigkeit: Restrict (NO ACTION) wie per EF DeleteBehavior.Restrict */
ALTER TABLE [dbo].[Taetigkeit]
    ADD CONSTRAINT [FK_Taetigkeit_Lehrer_LehrerID]
        FOREIGN KEY ([LehrerID]) REFERENCES [dbo].[Lehrer]([LehrerID]) ON DELETE NO ACTION;

ALTER TABLE [dbo].[Taetigkeit]
    ADD CONSTRAINT [FK_Taetigkeit_Projekt_ProjektID]
        FOREIGN KEY ([ProjektID]) REFERENCES [dbo].[Projekt]([ProjektID]) ON DELETE NO ACTION;

/* Performance-Indizes für FKs */
CREATE INDEX [IX_Taetigkeit_LehrerID] ON [dbo].[Taetigkeit]([LehrerID]);
CREATE INDEX [IX_Taetigkeit_ProjektID] ON [dbo].[Taetigkeit]([ProjektID]);

CREATE TABLE [dbo].[LehrerSchuljahrSollstunden]
(
    [LehrerID]    NVARCHAR(36)   NOT NULL,
    [SchuljahrID] INT            NOT NULL,
    [Sollstunden] DECIMAL(18,2)  NOT NULL CONSTRAINT [DF_LSS_Sollstunden] DEFAULT(0),
    CONSTRAINT [PK_LehrerSchuljahrSollstunden] PRIMARY KEY ([LehrerID], [SchuljahrID])
);

/* FKs für LSS: Cascade wie im EF-Model */
ALTER TABLE [dbo].[LehrerSchuljahrSollstunden]
    ADD CONSTRAINT [FK_LSS_Lehrer_LehrerID]
        FOREIGN KEY ([LehrerID]) REFERENCES [dbo].[Lehrer]([LehrerID]) ON DELETE CASCADE;

ALTER TABLE [dbo].[LehrerSchuljahrSollstunden]
    ADD CONSTRAINT [FK_LSS_Schuljahr_SchuljahrID]
        FOREIGN KEY ([SchuljahrID]) REFERENCES [dbo].[Schuljahr]([SchuljahrID]) ON DELETE CASCADE;

CREATE TABLE [dbo].[Abteilungsvorstand]
(
    [AbteilungsvorstandID] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Abteilungsvorstand] PRIMARY KEY,
    [AbteilungID]          INT                NOT NULL,
    [LehrerID]             NVARCHAR(36)       NOT NULL,
    [StartDatum]           DATE               NULL,
    [EndDatum]             DATE               NULL
);

/* FKs für Abteilungsvorstand: Restrict (NO ACTION) */
ALTER TABLE [dbo].[Abteilungsvorstand]
    ADD CONSTRAINT [FK_AV_Abteilung_AbteilungID]
        FOREIGN KEY ([AbteilungID]) REFERENCES [dbo].[Abteilung]([AbteilungID]) ON DELETE NO ACTION;

ALTER TABLE [dbo].[Abteilungsvorstand]
    ADD CONSTRAINT [FK_AV_Lehrer_LehrerID]
        FOREIGN KEY ([LehrerID]) REFERENCES [dbo].[Lehrer]([LehrerID]) ON DELETE NO ACTION;

/* Indizes für häufige Abfragen (Counts je Lehrer, je Abteilung) */
CREATE INDEX [IX_Abteilungsvorstand_LehrerID] ON [dbo].[Abteilungsvorstand]([LehrerID]);
CREATE INDEX [IX_Abteilungsvorstand_AbteilungID] ON [dbo].[Abteilungsvorstand]([AbteilungID]);

/* Optionale minimale Seeds – insbesondere Schuljahre (Controller referenziert SchuljahrID = 2) */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Schuljahr])
BEGIN
    SET IDENTITY_INSERT [dbo].[Schuljahr] ON;
    INSERT INTO [dbo].[Schuljahr] ([SchuljahrID],[Bezeichnung],[Startdatum],[Enddatum])
    VALUES (1, N'2024/25', '2024-09-01', '2025-08-31');
    INSERT INTO [dbo].[Schuljahr] ([SchuljahrID],[Bezeichnung],[Startdatum],[Enddatum])
    VALUES (2, N'2025/26', '2025-09-01', '2026-08-31');
    SET IDENTITY_INSERT [dbo].[Schuljahr] OFF;
END

/* Optional: Beispiel-Projekt/Abteilung (ohne feste IDs) */
IF NOT EXISTS (SELECT 1 FROM [dbo].[Projekt])
BEGIN
    INSERT INTO [dbo].[Projekt] ([Projektname]) VALUES (N'Allgemein');
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Abteilung])
BEGIN
    INSERT INTO [dbo].[Abteilung] ([Name],[Kuerzel],[IsActive]) VALUES (N'Informatik', N'INF', 1);
END

PRINT N'Datenbankschema wurde eingerichtet.';
PRINT N'LehrerID ist jetzt NVARCHAR(36) für Azure AD Object ID.';
