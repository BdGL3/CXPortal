DELETE FROM [EventAndStatsLog].[dbo].[LogTable]
      WHERE datetime <= DATEADD(d, -30, GETDATE())
GO


