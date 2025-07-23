USE master
GO

CREATE DATABASE GestionVoluntariadoEventos_DB05
GO

USE GestionVoluntariadoEventos_DB05
GO

CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(13) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL,
    TermsAccepted BIT NOT NULL DEFAULT 0
);

CREATE TABLE Events (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    DateTime DATETIME NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Description NVARCHAR(250) NOT NULL,
    DurationMinutes INT NOT NULL CHECK (DurationMinutes >= 5),
    SpecialRequirements NVARCHAR(500) NULL,
    VolunteersRequired INT NOT NULL CHECK (VolunteersRequired >= 1),
    OrganizerContact NVARCHAR(100) NOT NULL,
    CONSTRAINT CHK_FutureDate CHECK (DateTime >= GETDATE())
);

CREATE TABLE Volunteers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    BirthDate DATE NOT NULL,
    Age AS (DATEDIFF(YEAR, BirthDate, GETDATE())),
    Skills NVARCHAR(500) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(13) NOT NULL UNIQUE
);

CREATE TABLE AvailabilitySlots (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VolunteerId INT NOT NULL,
    DayOfWeek NVARCHAR(20) NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    FOREIGN KEY (VolunteerId) REFERENCES Volunteers(Id),
    CONSTRAINT CHK_TimeRange CHECK (EndTime > StartTime),
    CONSTRAINT CHK_MinDuration CHECK (DATEDIFF(MINUTE, StartTime, EndTime) >= 60)
);

CREATE TABLE EventVolunteers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    VolunteerId INT NOT NULL,
    FOREIGN KEY (EventId) REFERENCES Events(Id),
    FOREIGN KEY (VolunteerId) REFERENCES Volunteers(Id)
);

CREATE TABLE FavoriteEvents (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    EventId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    DateTime DATETIME NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Description NVARCHAR(250) NOT NULL,
    DurationMinutes INT NOT NULL,
    SpecialRequirements NVARCHAR(500) NULL,
    VolunteersRequired INT NOT NULL,
    OrganizerContact NVARCHAR(100) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);