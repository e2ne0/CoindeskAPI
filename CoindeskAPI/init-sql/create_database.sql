USE master;

IF DB_ID('CoindeskDB') IS NULL
BEGIN
    CREATE DATABASE CoindeskDB
    COLLATE Chinese_Taiwan_Stroke_CI_AI;
END