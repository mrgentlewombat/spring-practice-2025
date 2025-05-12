-- -- DeployDb.sql

-- -- Create database, if doesn't exist
-- CREATE DATABASE IF NOT EXISTS spp_masternode

-- -- Create the user
-- CREATE USER IF NOT EXISTS 'spp_user'@'%' IDENTIFIED BY 'spp_password';
-- GRANT ALL PRIVILEGES ON spp_masternode.* TO 'spp_user'@'%';
-- FLUSH PRIVILEGES;

-- Create database
CREATE DATABASE IF NOT EXISTS spp_masternode CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create user and grant privileges
CREATE USER IF NOT EXISTS 'spp_user'@'%' IDENTIFIED BY 'spp_password';
GRANT ALL PRIVILEGES ON spp_masternode.* TO 'spp_user'@'%';
FLUSH PRIVILEGES;

-- Select the database
USE spp_masternode;

-- Create Regions table
CREATE TABLE IF NOT EXISTS Regions (
    Id VARCHAR(10) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL
);

-- Insert seed data
INSERT INTO Regions (Id, Name) VALUES ('SU', 'Soviet Union');
INSERT INTO Regions (Id, Name) VALUES ('US', 'United States');
INSERT INTO Regions (Id, Name) VALUES ('DD', 'East Germany');
INSERT INTO Regions (Id, Name) VALUES ('CU', 'Cuba');
INSERT INTO Regions (Id, Name) VALUES ('CN', 'China');
INSERT INTO Regions (Id, Name) VALUES ('HU', 'Hungary');
INSERT INTO Regions (Id, Name) VALUES ('CA', 'Canada');
INSERT INTO Regions (Id, Name) VALUES ('SE', 'Sweden');
INSERT INTO Regions (Id, Name) VALUES ('NL', 'Netherlands');
INSERT INTO Regions (Id, Name) VALUES ('GB', 'Great Britain');
INSERT INTO Regions (Id, Name) VALUES ('BE', 'Belgium');
INSERT INTO Regions (Id, Name) VALUES ('NO', 'Norway');
INSERT INTO Regions (Id, Name) VALUES ('FR', 'France');
INSERT INTO Regions (Id, Name) VALUES ('FI', 'Finland');
INSERT INTO Regions (Id, Name) VALUES ('IT', 'Italy');
INSERT INTO Regions (Id, Name) VALUES ('DE', 'Germany');
INSERT INTO Regions (Id, Name) VALUES ('AT', 'Austria');
INSERT INTO Regions (Id, Name) VALUES ('RO', 'Romania');
INSERT INTO Regions (Id, Name) VALUES ('CH', 'Switzerland');
INSERT INTO Regions (Id, Name) VALUES ('AU', 'Australia');
