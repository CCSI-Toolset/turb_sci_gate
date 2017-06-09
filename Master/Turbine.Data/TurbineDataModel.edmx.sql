
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 09/16/2013 11:55:52
-- Generated from EDMX file: C:\Users\boverhof\Documents\Visual Studio 2012\Projects\turb_sci_gate\Master\Turbine.Data\TurbineDataModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_ApplicationOutputFileType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OutputFileTypes] DROP CONSTRAINT [FK_ApplicationOutputFileType];
GO
IF OBJECT_ID(N'[dbo].[FK_ApplicationInputFileType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InputFileTypes] DROP CONSTRAINT [FK_ApplicationInputFileType];
GO
IF OBJECT_ID(N'[dbo].[FK_SimulationApplication]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Simulations] DROP CONSTRAINT [FK_SimulationApplication];
GO
IF OBJECT_ID(N'[dbo].[FK_JobSinterProcess]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Jobs] DROP CONSTRAINT [FK_JobSinterProcess];
GO
IF OBJECT_ID(N'[dbo].[FK_StatgedOutputFileOutputFileType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StatgedOutputFiles] DROP CONSTRAINT [FK_StatgedOutputFileOutputFileType];
GO
IF OBJECT_ID(N'[dbo].[FK_StagedInputFileInputFileType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StagedInputFiles] DROP CONSTRAINT [FK_StagedInputFileInputFileType];
GO
IF OBJECT_ID(N'[dbo].[FK_SinterProcessError]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ProcessErrors] DROP CONSTRAINT [FK_SinterProcessError];
GO
IF OBJECT_ID(N'[dbo].[FK_JobMessage]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_JobMessage];
GO
IF OBJECT_ID(N'[dbo].[FK_SessionJob]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Jobs] DROP CONSTRAINT [FK_SessionJob];
GO
IF OBJECT_ID(N'[dbo].[FK_UserApplication]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Applications] DROP CONSTRAINT [FK_UserApplication];
GO
IF OBJECT_ID(N'[dbo].[FK_UserJob]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Jobs] DROP CONSTRAINT [FK_UserJob];
GO
IF OBJECT_ID(N'[dbo].[FK_UserSession]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Sessions] DROP CONSTRAINT [FK_UserSession];
GO
IF OBJECT_ID(N'[dbo].[FK_ConsumerJob]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Jobs] DROP CONSTRAINT [FK_ConsumerJob];
GO
IF OBJECT_ID(N'[dbo].[FK_SimulationStagedInputInputFileType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SimulationStagedInputs] DROP CONSTRAINT [FK_SimulationStagedInputInputFileType];
GO
IF OBJECT_ID(N'[dbo].[FK_JobStatgedOutputFile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StatgedOutputFiles] DROP CONSTRAINT [FK_JobStatgedOutputFile];
GO
IF OBJECT_ID(N'[dbo].[FK_JobStagedInputFile]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StagedInputFiles] DROP CONSTRAINT [FK_JobStagedInputFile];
GO
IF OBJECT_ID(N'[dbo].[FK_SimulationSimulationStagedInput]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[SimulationStagedInputs] DROP CONSTRAINT [FK_SimulationSimulationStagedInput];
GO
IF OBJECT_ID(N'[dbo].[FK_SimulationJob]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Jobs] DROP CONSTRAINT [FK_SimulationJob];
GO
IF OBJECT_ID(N'[dbo].[FK_UserSimulation]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Simulations] DROP CONSTRAINT [FK_UserSimulation];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Simulations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Simulations];
GO
IF OBJECT_ID(N'[dbo].[Jobs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Jobs];
GO
IF OBJECT_ID(N'[dbo].[SinterProcesses]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SinterProcesses];
GO
IF OBJECT_ID(N'[dbo].[ProcessErrors]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProcessErrors];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO
IF OBJECT_ID(N'[dbo].[JobConsumers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[JobConsumers];
GO
IF OBJECT_ID(N'[dbo].[Messages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Messages];
GO
IF OBJECT_ID(N'[dbo].[Sessions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Sessions];
GO
IF OBJECT_ID(N'[dbo].[StagedInputFiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StagedInputFiles];
GO
IF OBJECT_ID(N'[dbo].[StatgedOutputFiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StatgedOutputFiles];
GO
IF OBJECT_ID(N'[dbo].[InputFileTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InputFileTypes];
GO
IF OBJECT_ID(N'[dbo].[Applications]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Applications];
GO
IF OBJECT_ID(N'[dbo].[OutputFileTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[OutputFileTypes];
GO
IF OBJECT_ID(N'[dbo].[SimulationStagedInputs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SimulationStagedInputs];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Simulations'
CREATE TABLE [dbo].[Simulations] (
    [Name] nvarchar(50)  NOT NULL,
    [ApplicationName] nvarchar(50)  NOT NULL,
    [Update] datetime  NOT NULL,
    [Create] datetime  NOT NULL,
    [Id] uniqueidentifier  NOT NULL,
    [UserName] nvarchar(50)  NULL
);
GO

-- Creating table 'Jobs'
CREATE TABLE [dbo].[Jobs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [State] nvarchar(max)  NOT NULL,
    [Create] datetime  NOT NULL,
    [Submit] datetime  NULL,
    [Setup] datetime  NULL,
    [Running] datetime  NULL,
    [Finished] datetime  NULL,
    [Initialize] bit  NOT NULL,
    [guid] uniqueidentifier  NOT NULL,
    [SessionId] uniqueidentifier  NOT NULL,
    [UserName] nvarchar(50)  NOT NULL,
    [ConsumerId] uniqueidentifier  NULL,
    [Reset] bit  NOT NULL,
    [SimulationId] uniqueidentifier  NOT NULL,
    [Process_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'SinterProcesses'
CREATE TABLE [dbo].[SinterProcesses] (
    [Id] uniqueidentifier  NOT NULL,
    [Status] int  NULL,
    [Stdout] varbinary(max)  NULL,
    [Stderr] varbinary(max)  NULL,
    [WorkingDir] nvarchar(max)  NULL,
    [Input] nvarchar(max)  NULL,
    [Output] nvarchar(max)  NULL
);
GO

-- Creating table 'ProcessErrors'
CREATE TABLE [dbo].[ProcessErrors] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Error] nvarchar(max)  NOT NULL,
    [Type] nvarchar(max)  NOT NULL,
    [SinterProcessId] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Name] nvarchar(50)  NOT NULL,
    [Token] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'JobConsumers'
CREATE TABLE [dbo].[JobConsumers] (
    [Id] uniqueidentifier  NOT NULL,
    [hostname] nvarchar(max)  NOT NULL,
    [Memory] bigint  NULL,
    [Cores] int  NULL,
    [CPU] nvarchar(max)  NULL,
    [AMI] nvarchar(max)  NULL,
    [image] nvarchar(max)  NULL,
    [instance] nvarchar(max)  NULL,
    [status] nvarchar(max)  NOT NULL,
    [processId] nvarchar(max)  NULL,
    [keepalive] datetime  NULL
);
GO

-- Creating table 'Messages'
CREATE TABLE [dbo].[Messages] (
    [Id] uniqueidentifier  NOT NULL,
    [Value] nvarchar(max)  NOT NULL,
    [JobId] int  NOT NULL
);
GO

-- Creating table 'Sessions'
CREATE TABLE [dbo].[Sessions] (
    [Id] uniqueidentifier  NOT NULL,
    [Create] datetime  NOT NULL,
    [Submit] datetime  NULL,
    [Finished] datetime  NULL,
    [UserName] nvarchar(50)  NOT NULL,
    [Descrption] nvarchar(max)  NULL
);
GO

-- Creating table 'StagedInputFiles'
CREATE TABLE [dbo].[StagedInputFiles] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Content] varbinary(max)  NOT NULL,
    [JobId] int  NOT NULL,
    [InputFileType_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'StatgedOutputFiles'
CREATE TABLE [dbo].[StatgedOutputFiles] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Content] varbinary(max)  NOT NULL,
    [JobId] int  NOT NULL,
    [OutputFileType_Id] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'InputFileTypes'
CREATE TABLE [dbo].[InputFileTypes] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(max)  NULL,
    [Required] bit  NOT NULL,
    [Type] nvarchar(max)  NOT NULL,
    [ApplicationName] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Applications'
CREATE TABLE [dbo].[Applications] (
    [Name] nvarchar(50)  NOT NULL,
    [Version] nvarchar(max)  NULL,
    [UserName] nvarchar(50)  NULL
);
GO

-- Creating table 'OutputFileTypes'
CREATE TABLE [dbo].[OutputFileTypes] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Required] bit  NOT NULL,
    [Type] nvarchar(max)  NOT NULL,
    [ApplicationName] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'SimulationStagedInputs'
CREATE TABLE [dbo].[SimulationStagedInputs] (
    [Id] uniqueidentifier  NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Content] varbinary(max)  NULL,
    [Hash] nchar(32)  NULL,
    [SimulationId] uniqueidentifier  NOT NULL,
    [InputFileType_Id] uniqueidentifier  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Simulations'
ALTER TABLE [dbo].[Simulations]
ADD CONSTRAINT [PK_Simulations]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Jobs'
ALTER TABLE [dbo].[Jobs]
ADD CONSTRAINT [PK_Jobs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SinterProcesses'
ALTER TABLE [dbo].[SinterProcesses]
ADD CONSTRAINT [PK_SinterProcesses]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ProcessErrors'
ALTER TABLE [dbo].[ProcessErrors]
ADD CONSTRAINT [PK_ProcessErrors]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Name] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Name] ASC);
GO

-- Creating primary key on [Id] in table 'JobConsumers'
ALTER TABLE [dbo].[JobConsumers]
ADD CONSTRAINT [PK_JobConsumers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [PK_Messages]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Sessions'
ALTER TABLE [dbo].[Sessions]
ADD CONSTRAINT [PK_Sessions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'StagedInputFiles'
ALTER TABLE [dbo].[StagedInputFiles]
ADD CONSTRAINT [PK_StagedInputFiles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'StatgedOutputFiles'
ALTER TABLE [dbo].[StatgedOutputFiles]
ADD CONSTRAINT [PK_StatgedOutputFiles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'InputFileTypes'
ALTER TABLE [dbo].[InputFileTypes]
ADD CONSTRAINT [PK_InputFileTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Name] in table 'Applications'
ALTER TABLE [dbo].[Applications]
ADD CONSTRAINT [PK_Applications]
    PRIMARY KEY CLUSTERED ([Name] ASC);
GO

-- Creating primary key on [Id] in table 'OutputFileTypes'
ALTER TABLE [dbo].[OutputFileTypes]
ADD CONSTRAINT [PK_OutputFileTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SimulationStagedInputs'
ALTER TABLE [dbo].[SimulationStagedInputs]
ADD CONSTRAINT [PK_SimulationStagedInputs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [ApplicationName] in table 'OutputFileTypes'
ALTER TABLE [dbo].[OutputFileTypes]
ADD CONSTRAINT [FK_ApplicationOutputFileType]
    FOREIGN KEY ([ApplicationName])
    REFERENCES [dbo].[Applications]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ApplicationOutputFileType'
CREATE INDEX [IX_FK_ApplicationOutputFileType]
ON [dbo].[OutputFileTypes]
    ([ApplicationName]);
GO

-- Creating foreign key on [ApplicationName] in table 'InputFileTypes'
ALTER TABLE [dbo].[InputFileTypes]
ADD CONSTRAINT [FK_ApplicationInputFileType]
    FOREIGN KEY ([ApplicationName])
    REFERENCES [dbo].[Applications]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ApplicationInputFileType'
CREATE INDEX [IX_FK_ApplicationInputFileType]
ON [dbo].[InputFileTypes]
    ([ApplicationName]);
GO

-- Creating foreign key on [ApplicationName] in table 'Simulations'
ALTER TABLE [dbo].[Simulations]
ADD CONSTRAINT [FK_SimulationApplication]
    FOREIGN KEY ([ApplicationName])
    REFERENCES [dbo].[Applications]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SimulationApplication'
CREATE INDEX [IX_FK_SimulationApplication]
ON [dbo].[Simulations]
    ([ApplicationName]);
GO

-- Creating foreign key on [Process_Id] in table 'Jobs'
ALTER TABLE [dbo].[Jobs]
ADD CONSTRAINT [FK_JobSinterProcess]
    FOREIGN KEY ([Process_Id])
    REFERENCES [dbo].[SinterProcesses]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_JobSinterProcess'
CREATE INDEX [IX_FK_JobSinterProcess]
ON [dbo].[Jobs]
    ([Process_Id]);
GO

-- Creating foreign key on [OutputFileType_Id] in table 'StatgedOutputFiles'
ALTER TABLE [dbo].[StatgedOutputFiles]
ADD CONSTRAINT [FK_StatgedOutputFileOutputFileType]
    FOREIGN KEY ([OutputFileType_Id])
    REFERENCES [dbo].[OutputFileTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_StatgedOutputFileOutputFileType'
CREATE INDEX [IX_FK_StatgedOutputFileOutputFileType]
ON [dbo].[StatgedOutputFiles]
    ([OutputFileType_Id]);
GO

-- Creating foreign key on [InputFileType_Id] in table 'StagedInputFiles'
ALTER TABLE [dbo].[StagedInputFiles]
ADD CONSTRAINT [FK_StagedInputFileInputFileType]
    FOREIGN KEY ([InputFileType_Id])
    REFERENCES [dbo].[InputFileTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_StagedInputFileInputFileType'
CREATE INDEX [IX_FK_StagedInputFileInputFileType]
ON [dbo].[StagedInputFiles]
    ([InputFileType_Id]);
GO

-- Creating foreign key on [SinterProcessId] in table 'ProcessErrors'
ALTER TABLE [dbo].[ProcessErrors]
ADD CONSTRAINT [FK_SinterProcessError]
    FOREIGN KEY ([SinterProcessId])
    REFERENCES [dbo].[SinterProcesses]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SinterProcessError'
CREATE INDEX [IX_FK_SinterProcessError]
ON [dbo].[ProcessErrors]
    ([SinterProcessId]);
GO

-- Creating foreign key on [JobId] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_JobMessage]
    FOREIGN KEY ([JobId])
    REFERENCES [dbo].[Jobs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_JobMessage'
CREATE INDEX [IX_FK_JobMessage]
ON [dbo].[Messages]
    ([JobId]);
GO

-- Creating foreign key on [SessionId] in table 'Jobs'
ALTER TABLE [dbo].[Jobs]
ADD CONSTRAINT [FK_SessionJob]
    FOREIGN KEY ([SessionId])
    REFERENCES [dbo].[Sessions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SessionJob'
CREATE INDEX [IX_FK_SessionJob]
ON [dbo].[Jobs]
    ([SessionId]);
GO

-- Creating foreign key on [UserName] in table 'Applications'
ALTER TABLE [dbo].[Applications]
ADD CONSTRAINT [FK_UserApplication]
    FOREIGN KEY ([UserName])
    REFERENCES [dbo].[Users]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserApplication'
CREATE INDEX [IX_FK_UserApplication]
ON [dbo].[Applications]
    ([UserName]);
GO

-- Creating foreign key on [UserName] in table 'Jobs'
ALTER TABLE [dbo].[Jobs]
ADD CONSTRAINT [FK_UserJob]
    FOREIGN KEY ([UserName])
    REFERENCES [dbo].[Users]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserJob'
CREATE INDEX [IX_FK_UserJob]
ON [dbo].[Jobs]
    ([UserName]);
GO

-- Creating foreign key on [UserName] in table 'Sessions'
ALTER TABLE [dbo].[Sessions]
ADD CONSTRAINT [FK_UserSession]
    FOREIGN KEY ([UserName])
    REFERENCES [dbo].[Users]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserSession'
CREATE INDEX [IX_FK_UserSession]
ON [dbo].[Sessions]
    ([UserName]);
GO

-- Creating foreign key on [ConsumerId] in table 'Jobs'
ALTER TABLE [dbo].[Jobs]
ADD CONSTRAINT [FK_ConsumerJob]
    FOREIGN KEY ([ConsumerId])
    REFERENCES [dbo].[JobConsumers]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ConsumerJob'
CREATE INDEX [IX_FK_ConsumerJob]
ON [dbo].[Jobs]
    ([ConsumerId]);
GO

-- Creating foreign key on [InputFileType_Id] in table 'SimulationStagedInputs'
ALTER TABLE [dbo].[SimulationStagedInputs]
ADD CONSTRAINT [FK_SimulationStagedInputInputFileType]
    FOREIGN KEY ([InputFileType_Id])
    REFERENCES [dbo].[InputFileTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SimulationStagedInputInputFileType'
CREATE INDEX [IX_FK_SimulationStagedInputInputFileType]
ON [dbo].[SimulationStagedInputs]
    ([InputFileType_Id]);
GO

-- Creating foreign key on [JobId] in table 'StatgedOutputFiles'
ALTER TABLE [dbo].[StatgedOutputFiles]
ADD CONSTRAINT [FK_JobStatgedOutputFile]
    FOREIGN KEY ([JobId])
    REFERENCES [dbo].[Jobs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_JobStatgedOutputFile'
CREATE INDEX [IX_FK_JobStatgedOutputFile]
ON [dbo].[StatgedOutputFiles]
    ([JobId]);
GO

-- Creating foreign key on [JobId] in table 'StagedInputFiles'
ALTER TABLE [dbo].[StagedInputFiles]
ADD CONSTRAINT [FK_JobStagedInputFile]
    FOREIGN KEY ([JobId])
    REFERENCES [dbo].[Jobs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_JobStagedInputFile'
CREATE INDEX [IX_FK_JobStagedInputFile]
ON [dbo].[StagedInputFiles]
    ([JobId]);
GO

-- Creating foreign key on [SimulationId] in table 'SimulationStagedInputs'
ALTER TABLE [dbo].[SimulationStagedInputs]
ADD CONSTRAINT [FK_SimulationSimulationStagedInput]
    FOREIGN KEY ([SimulationId])
    REFERENCES [dbo].[Simulations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SimulationSimulationStagedInput'
CREATE INDEX [IX_FK_SimulationSimulationStagedInput]
ON [dbo].[SimulationStagedInputs]
    ([SimulationId]);
GO

-- Creating foreign key on [SimulationId] in table 'Jobs'
ALTER TABLE [dbo].[Jobs]
ADD CONSTRAINT [FK_SimulationJob]
    FOREIGN KEY ([SimulationId])
    REFERENCES [dbo].[Simulations]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_SimulationJob'
CREATE INDEX [IX_FK_SimulationJob]
ON [dbo].[Jobs]
    ([SimulationId]);
GO

-- Creating foreign key on [UserName] in table 'Simulations'
ALTER TABLE [dbo].[Simulations]
ADD CONSTRAINT [FK_UserSimulation]
    FOREIGN KEY ([UserName])
    REFERENCES [dbo].[Users]
        ([Name])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserSimulation'
CREATE INDEX [IX_FK_UserSimulation]
ON [dbo].[Simulations]
    ([UserName]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------