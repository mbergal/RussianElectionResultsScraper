create table  VotingResult
	(
	Id				int				NOT NULL,
	Counter			nvarchar(3)		NULL,
	Value			int				NULL,
	VotingPlaceId	nvarchar(32)	NULL,
	Message			nvarchar(255)	NULL,
	Percents		decimal(5, 2)	NULL,
	IsCalculated	int				NULL,
	PRIMARY KEY NONCLUSTERED 
		( 
		Id ASC
		)
	)

create clustered index IX_VotingResult_VotingPlaceId on VotingResult
	(
	VotingPlaceId ASC,
	Id			  ASC
	)

create nonclustered index IX_IsCalculated on VotingResult
	(
	IsCalculated ASC
	)
	

create nonclustered index IX_VotingResult_Message on VotingResult
	(
	Message      ASC
	)

create table VotingPlace
	(
	Id															nvarchar(32)		NOT NULL,
	Name														nvarchar(255)		NULL,
	FullName													nvarchar(255)		NULL,
	Uri															nvarchar(255)		NULL,
	Type														int					NULL,
	NumberOfVotersInVoterList									int					NULL,
	NumberOfBallotsReceivedByElectoralComission					int					NULL,
	NumberOfBallotsIssuedToVotersWhoVotedEarly					int					NULL,
	NumberOfBallotsIssuedToVoterAtPollStation					int					NULL,
	NumberOfBallotsIssuedToVotersOutsideOfPollStation			int					NULL,
	NumberOfCancelledBallots									int					NULL,
	NumberOfBallotsInPortableBallotBoxes						int					NULL,
	NumberOfBallotsInStationaryBallotBoxes						int					NULL,
	NumberOfInvalidBallots										int					NULL,
	NumberValidBallots											int					NULL,
	NumberOfAbsenteePermitsReceivedByElectoralComission			int					NULL,
	NumberOfAbsenteePermitsIssuedAtPollingStation				int					NULL,
	NumberOfAbsenteeBallotsCastAtPollingStation					int					NULL,
	NumberOfCanceledAbsenteePermits								int					NULL,
	NumberOfAbsenteeBallotsIssuedByTerritorialElectionComission int					NULL,
	NumberOfLostAbsenteeBallots									int					NULL,
	NumberOfLostBallots											int					NULL,
	NumberOfBallotsNotCounterUponReceiving						int					NULL,
	Attendance													float				NULL,
	Path														nvarchar(255)		NULL,
	ParentId													nvarchar(32)		NULL,
	ElectionId													nvarchar(32)		NULL,
	NumberOfErrors												int					NOT NULL,
	CONSTRAINT [PK_Id] PRIMARY KEY NONCLUSTERED 
		(
		[Id] ASC
		)
	)

create clustered index IX_VotingPlace_ParentId_Id ON VotingPlace
	(
	ParentId ASC,
	Id ASC
	)


CREATE NONCLUSTERED INDEX IX_VotingPlace_NumberOfErrors ON VotingPlace
	(
	NumberOfErrors ASC
	)



CREATE NONCLUSTERED INDEX IX_VotingPlace_ElectionId_FullName ON VotingPlace
	(
	ElectionId	ASC,
	FullName	ASC
	)


CREATE NONCLUSTERED INDEX IX_VotingPlace_ElectionId_Type ON VotingPlace
	(
	ElectionId ASC,
	Type	ASC
	)


CREATE NONCLUSTERED INDEX IX_VotingPlace_Path_Id ON VotingPlace
	(
	Path ASC,
	Id ASC
	)

CREATE TABLE Election
	(
	Id						nvarchar(32) NOT NULL,
	Name					nvarchar(255) NULL,
	LastUpdateTimeStamp 	datetime NULL,
	RootId					nvarchar(32) NULL,
	CONSTRAINT [PK_Election_Id]
	PRIMARY KEY CLUSTERED 
		(
		Id asc
		)
	)

CREATE TABLE CounterDescription
	(
	Id						int NOT NULL,
	Counter					nvarchar(3) NULL,
	Name					nvarchar(128) NULL,
	ShortName				nvarchar(16) NULL,
	Color					nvarchar(32) NULL,
	ElectionId				nvarchar(32) NULL,
	IsCandidate				int NULL
	CONSTRAINT PK_CounterDescription PRIMARY KEY CLUSTERED 
		(
		Id ASC
		)
	)

ALTER TABLE VotingPlace ADD CONSTRAINT [DF_VotingPlace_NumberOfErrors]  DEFAULT ((0)) FOR NumberOfErrors
ALTER TABLE VotingResult WITH CHECK ADD CONSTRAINT FK_VotingResult_VotingPlace_VotingPlaceId FOREIGN KEY(VotingPlaceId) REFERENCES VotingPlace ( Id )
ALTER TABLE VotingPlace  WITH CHECK ADD CONSTRAINT FK_VotingPlace_Election_Id_Election FOREIGN KEY(ElectionId) REFERENCES Election ( Id )
ALTER TABLE VotingPlace  WITH CHECK ADD CONSTRAINT FK_VotingPlace_VotingPlace_ParentId FOREIGN KEY(ParentId) REFERENCES VotingPlace (Id)
ALTER TABLE Election     WITH CHECK ADD  CONSTRAINT FK_Election_Root_Id_Voting_Place FOREIGN KEY(RootId) REFERENCES VotingPlace (Id)
ALTER TABLE CounterDescription WITH CHECK ADD CONSTRAINT FK_Counter_Description_Election_Id_Election  FOREIGN KEY(ElectionId) REFERENCES Election(Id)

