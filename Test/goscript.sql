CREATE TABLE [example] ([pdfTikaContents] ntext NOT NULL);
GO

INSERT INTO [example] ([pdfTikaContents]) VALUES (N'PDF contents extracted by Apache Tika...
  we    
  go    
  in    
  a    
  different    
  direction    
  ...');
GO
-- A comment
SELECT *
FROM [example]
GO
-- Last comment
