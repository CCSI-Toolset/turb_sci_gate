SET QUOTED_IDENTIFIER OFF;

--GO
--USE [turbineStandAloneMS10];
--GO

DELETE FROM dbo.SimulationStagedInputs;
DELETE FROM dbo.InputFileTypes;
DELETE FROM dbo.Simulations;
DELETE FROM dbo.Applications;

INSERT INTO dbo.Applications VALUES ('AspenPlus', '0.1', null);
INSERT INTO dbo.InputFileTypes VALUES ( newid(), 'aspenfile', 1, 'text/plain', 'AspenPlus');
INSERT INTO dbo.InputFileTypes VALUES ( newid(), 'configuration', 1, 'text/plain', 'AspenPlus');

INSERT INTO dbo.Applications VALUES ('Excel', '0.1', null);
INSERT INTO dbo.InputFileTypes VALUES ( newid(), 'aspenfile', 1, 'application/zip', 'Excel');
INSERT INTO dbo.InputFileTypes VALUES ( newid(), 'configuration', 1, 'text/plain', 'Excel');

INSERT INTO dbo.Applications VALUES ('ACM', '0.1', null);
INSERT INTO dbo.InputFileTypes VALUES ( newid(), 'aspenfile', 1, 'text/plain', 'ACM');
INSERT INTO dbo.InputFileTypes VALUES ( newid(), 'configuration', 1, 'text/plain', 'ACM');