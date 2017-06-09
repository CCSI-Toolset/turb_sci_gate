-- -- Create Tables


-- Creating table 'Users'
CREATE TABLE Users (
    Id      SERIAL primary key unique NOT NULL,
    Name    varchar NOT NULL,
    Token   varchar NOT NULL
);

-- Creating table 'Consumers'
CREATE TABLE Consumers (
    guid      uuid primary key unique NOT NULL,
    hostname  varchar NOT NULL,
    Memory    int,
    Cores     int,
    CPU       varchar,
    AMI       varchar,
    image     varchar,
    instance  varchar,
    status    varchar NOT NULL
);


-- Creating table 'Sessions'
CREATE TABLE Sessions (
    guid      uuid primary key unique  NOT NULL,
    "Create"  timestamp NOT NULL,
    Submit    timestamp,
    Finished  timestamp,
    UserId    int NOT NULL
    	references Users(Id)
);

create index IX_Sessions_UserId
		on Sessions(UserId);


-- Creating table 'SinterProcesses'
CREATE TABLE SinterProcesses (
    Id              SERIAL primary key unique NOT NULL,
    Status          int,
    Stdout          varchar,
    Stderr          varchar,
    WorkingDir      varchar,
    Configuration   varchar,
    Backup          varchar,
    Input           varchar,
    Output          varchar
);

-- Creating table 'Consumers_JobConsumer'
CREATE TABLE Consumers_JobConsumer (
    guid uuid primary key unique NOT NULL
    	references Consumers(guid)
);


-- Creating table 'Simulations'
CREATE TABLE Simulations (
    Id              SERIAL primary key unique NOT NULL,
    Name            varchar NOT NULL,
    Configuration   varchar,
    Backup          varchar,
    Defaults        varchar,
    UserId          int NOT NULL
    	references Users(Id)
);

create index IX_Simulations_UserId
	on Simulations(UserId);


-- Creating table 'Jobs'
CREATE TABLE Jobs (
    Id                  SERIAL primary key unique NOT NULL,
    State               varchar NOT NULL,
    "Create"            timestamp  NOT NULL,
    Submit              timestamp,
    Setup               timestamp,
    Running             timestamp,
    Finished            timestamp,
    SimulationId        int NOT NULL
        references Simulations(Id),
    UserId              int NOT NULL
    	references Users(Id),
    SessionGuid        uuid NOT NULL
    	references Sessions(guid),
    JobConsumerGuid    uuid
    	references Consumers_JobConsumer(guid),
    ProcessId          int NOT NULL
        references SinterProcesses(Id)
);

create index IX_Jobs_UserId
	on Jobs(UserId);

create index IX_Jobs_SimulationId
        on Jobs(SimulationId);

create index IX_Jobs_Process_Id
        on Jobs(ProcessId);

create index IX_Jobs_SessionGuid
		on Jobs(SessionGuid);

create index IX_Jobs_JObConsumerGuid
		on Jobs(JobConsumerGuid);
-- Creating table 'AspenProcessErrors'
CREATE TABLE AspenProcessErrors (
    Id              SERIAL primary key unique NOT NULL,
    Name            varchar NOT NULL,
    Error           varchar NOT NULL,
    Type            varchar NOT NULL,
    SinterProcessId int  NOT NULL
        references SinterProcesses(Id)
);

create index IX_AspenProcessErrors_SinterProcessId
        on AspenProcessErrors(SinterProcessId);


-- Creating table 'Messages'
CREATE TABLE Messages (
    Id    SERIAL primary key unique NOT NULL,
    Value varchar NOT NULL,
    JobId int NOT NULL
    	references Jobs(Id)
);

create index IX_Messages_JobId
		on Messages(JobId);

-- End
