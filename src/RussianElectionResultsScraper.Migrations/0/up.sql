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


CREATE NONCLUSTERED INDEX [_dta_index_VotingPlace_36_1657772963__K28] ON [dbo].[VotingPlace] 
(
	[NumberOfErrors] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]



CREATE NONCLUSTERED INDEX [IX_ElectionId_FullName] ON [dbo].[VotingPlace] 
(
	[ElectionId] ASC,
	[FullName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_ElectionId_Type] ON [dbo].[VotingPlace] 
(
	[ElectionId] ASC,
	[Type] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_VotingPlace_Path_Id] ON [dbo].[VotingPlace] 
(
	[Path] ASC,
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Election]    Script Date: 03/24/2012 18:35:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Election](
	[Id] [nvarchar](32) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[LastUpdateTimeStamp] [datetime] NULL,
	[RootId] [nvarchar](32) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CounterDescription]    Script Date: 03/24/2012 18:35:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CounterDescription](
	[Id] [int] NOT NULL,
	[Counter] [nvarchar](3) NULL,
	[Name] [nvarchar](128) NULL,
	[ShortName] [nvarchar](16) NULL,
	[Color] [nvarchar](32) NULL,
	[ElectionId] [nvarchar](32) NULL,
	[IsCandidate] [int] NULL,
 CONSTRAINT [PK__CounterD__3214EC07688874F9] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Default [DF_VotingPlace_NumberOfErrors]    Script Date: 03/24/2012 18:35:40 ******/
ALTER TABLE [dbo].[VotingPlace] ADD  CONSTRAINT [DF_VotingPlace_NumberOfErrors]  DEFAULT ((0)) FOR [NumberOfErrors]
GO
/****** Object:  ForeignKey [FK58BD4584D5E6B0C3]    Script Date: 03/24/2012 18:35:40 ******/
ALTER TABLE [dbo].[VotingResult]  WITH CHECK ADD  CONSTRAINT [FK58BD4584D5E6B0C3] FOREIGN KEY([VotingPlaceId])
REFERENCES [dbo].[VotingPlace] ([Id])
GO
ALTER TABLE [dbo].[VotingResult] CHECK CONSTRAINT [FK58BD4584D5E6B0C3]
GO
/****** Object:  ForeignKey [FK7D703BF419D2DA1D]    Script Date: 03/24/2012 18:35:40 ******/
ALTER TABLE [dbo].[VotingPlace]  WITH CHECK ADD  CONSTRAINT [FK7D703BF419D2DA1D] FOREIGN KEY([ElectionId])
REFERENCES [dbo].[Election] ([Id])
GO
ALTER TABLE [dbo].[VotingPlace] CHECK CONSTRAINT [FK7D703BF419D2DA1D]
GO
/****** Object:  ForeignKey [FK7D703BF4CE5556B5]    Script Date: 03/24/2012 18:35:40 ******/
ALTER TABLE [dbo].[VotingPlace]  WITH CHECK ADD  CONSTRAINT [FK7D703BF4CE5556B5] FOREIGN KEY([ParentId])
REFERENCES [dbo].[VotingPlace] ([Id])
GO
ALTER TABLE [dbo].[VotingPlace] CHECK CONSTRAINT [FK7D703BF4CE5556B5]
GO
/****** Object:  ForeignKey [FK944ACA1F91F56413]    Script Date: 03/24/2012 18:35:40 ******/
ALTER TABLE [dbo].[Election]  WITH CHECK ADD  CONSTRAINT [FK944ACA1F91F56413] FOREIGN KEY([RootId])
REFERENCES [dbo].[VotingPlace] ([Id])
GO
ALTER TABLE [dbo].[Election] CHECK CONSTRAINT [FK944ACA1F91F56413]
GO
/****** Object:  ForeignKey [FKE75D3C2019D2DA1D]    Script Date: 03/24/2012 18:35:40 ******/
ALTER TABLE [dbo].[CounterDescription]  WITH CHECK ADD  CONSTRAINT [FKE75D3C2019D2DA1D] FOREIGN KEY([ElectionId])
REFERENCES [dbo].[Election] ([Id])
GO
ALTER TABLE [dbo].[CounterDescription] CHECK CONSTRAINT [FKE75D3C2019D2DA1D]
GO
