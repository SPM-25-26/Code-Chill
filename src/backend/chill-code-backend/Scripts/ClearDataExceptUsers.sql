-- Clear all data except Users and Preferences
-- Run this script to reset the database while keeping user accounts
-- Database: SQLite

-- Disable foreign key constraints temporarily
PRAGMA foreign_keys = OFF;

-- Delete all POI-related data
DELETE FROM PointsOfInterest;
DELETE FROM Events;
DELETE FROM Routes;
DELETE FROM Articles;
DELETE FROM Municipalities;
DELETE FROM IngestionRuns;

-- Keep Users, UserPreferences, and PreferenceResponses intact

-- Re-enable foreign key constraints
PRAGMA foreign_keys = ON;

-- Vacuum to reclaim disk space
VACUUM;

-- Show summary
SELECT 'Data cleared successfully. Users and preferences preserved.' AS Status;
