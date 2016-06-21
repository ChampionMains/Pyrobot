
CREATE TABLE [dbo].[SummonerChampionMastery]
(
	[Id] INT NOT NULL IDENTITY,
	[MasteryPoints] INT NOT NULL,
	[MasteryLevel] TINYINT NOT NULL,

	[ChampionId] SMALLINT NOT NULL,
	[SummonerInfoId] INT NOT NULL,

	CONSTRAINT PK_SummonerChampionMastery PRIMARY KEY ([Id]),
	CONSTRAINT FK_SummonerChampionMastery_Champion FOREIGN KEY ([ChampionId]) REFERENCES [dbo].[Champion] ([Id]),
	CONSTRAINT FK_SummonerChampionMastery_SummonerInfo FOREIGN KEY ([SummonerInfoId]) REFERENCES [dbo].[SummonerInfo] ([Id])
)
