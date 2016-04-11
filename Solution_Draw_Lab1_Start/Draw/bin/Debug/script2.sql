use [819523465] 
/****** Object:  Table [dbo].[Draw_FreeLine]    Script Date: 3/28/2016 9:05:48 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Draw_FreeLine](
	[DrawId] [int] NOT NULL,
	[FreeLineID] [int] NOT NULL
) ON [PRIMARY]


/****** Object:  Table [dbo].[Draw_Text]    Script Date: 3/28/2016 9:05:48 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Draw_Text](
	[DrawID] [int] NOT NULL,
	[TextID] [int] NOT NULL
) ON [PRIMARY]


/****** Object:  Table [dbo].[Drawing]    Script Date: 3/28/2016 9:05:48 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Drawing](
	[Drawing_Name] [nvarchar](50) NOT NULL,
	[Drawing_ID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Drawing] PRIMARY KEY CLUSTERED 
(
	[Drawing_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


/****** Object:  Table [dbo].[FreeLine]    Script Date: 3/28/2016 9:05:48 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[FreeLine](
	[FreeLineId] [int] IDENTITY(1,1) NOT NULL,
	[Color] [varchar](50) NULL,
	[PenWidth] [int] NULL,
 CONSTRAINT [PK_FreeLine] PRIMARY KEY CLUSTERED 
(
	[FreeLineId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


SET ANSI_PADDING OFF

/****** Object:  Table [dbo].[FreeLine_Point]    Script Date: 3/28/2016 9:05:48 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[FreeLine_Point](
	[FreeLineID] [int] NOT NULL,
	[PointID] [int] NOT NULL
) ON [PRIMARY]


/****** Object:  Table [dbo].[Line]    Script Date: 3/28/2016 9:05:48 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Line](
	[ShapeID] [int] IDENTITY(1,1) NOT NULL,
	[PictureID] [int] NOT NULL,
	[X1] [real] NULL,
	[Y1] [real] NULL,
	[X2] [real] NULL,
	[Y2] [real] NULL,
	[Color] [varchar](50) NULL,
	[PenWidth] [int] NULL,
 CONSTRAINT [PK_Line] PRIMARY KEY CLUSTERED 
(
	[ShapeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


/****** Object:  Table [dbo].[Point]    Script Date: 3/28/2016 9:05:48 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Point](
	[PointID] [int] IDENTITY(1,1) NOT NULL,
	[X] [int] NULL,
	[Y] [int] NULL,
 CONSTRAINT [PK_Point] PRIMARY KEY CLUSTERED 
(
	[PointID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


/****** Object:  Table [dbo].[Rectangle]    Script Date: 3/28/2016 9:05:48 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Rectangle](
	[X1] [real] NULL,
	[Y1] [real] NULL,
	[X2] [real] NULL,
	[Y2] [real] NULL,
	[PictureID] [int] NOT NULL,
	[Color] [varchar](50) NULL,
	[PenWidth] [int] NULL,
	[ShapeID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_Rectangle] PRIMARY KEY CLUSTERED 
(
	[ShapeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


/****** Object:  Table [dbo].[Text]    Script Date: 3/28/2016 9:05:48 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

SET ANSI_PADDING ON

CREATE TABLE [dbo].[Text](
	[TextID] [int] IDENTITY(1,1) NOT NULL,
	[TextData] [nvarchar](max) NULL,
	[PenWidth] [int] NULL,
	[Color] [varchar](50) NULL,
	[FontFamily] [varchar](50) NULL,
	[FontSize] [int] NULL,
	[X] [int] NULL,
	[Y] [int] NULL,
 CONSTRAINT [PK_Text] PRIMARY KEY CLUSTERED 
(
	[TextID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]


SET ANSI_PADDING OFF

SET ANSI_PADDING ON


/****** Object:  Index [IX_Drawing]    Script Date: 3/28/2016 9:05:48 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Drawing] ON [dbo].[Drawing]
(
	[Drawing_Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

ALTER TABLE [dbo].[Draw_FreeLine]  WITH CHECK ADD  CONSTRAINT [FK_Draw_FreeLine_Drawing] FOREIGN KEY([DrawId])
REFERENCES [dbo].[Drawing] ([Drawing_ID])

ALTER TABLE [dbo].[Draw_FreeLine] CHECK CONSTRAINT [FK_Draw_FreeLine_Drawing]

ALTER TABLE [dbo].[Draw_FreeLine]  WITH CHECK ADD  CONSTRAINT [FK_Draw_FreeLine_FreeLine] FOREIGN KEY([FreeLineID])
REFERENCES [dbo].[FreeLine] ([FreeLineId])

ALTER TABLE [dbo].[Draw_FreeLine] CHECK CONSTRAINT [FK_Draw_FreeLine_FreeLine]

ALTER TABLE [dbo].[Draw_Text]  WITH CHECK ADD  CONSTRAINT [FK_Draw_Text_Drawing] FOREIGN KEY([DrawID])
REFERENCES [dbo].[Drawing] ([Drawing_ID])
ON DELETE CASCADE

ALTER TABLE [dbo].[Draw_Text] CHECK CONSTRAINT [FK_Draw_Text_Drawing]

ALTER TABLE [dbo].[Draw_Text]  WITH CHECK ADD  CONSTRAINT [FK_Draw_Text_Text] FOREIGN KEY([TextID])
REFERENCES [dbo].[Text] ([TextID])
ON DELETE CASCADE

ALTER TABLE [dbo].[Draw_Text] CHECK CONSTRAINT [FK_Draw_Text_Text]

ALTER TABLE [dbo].[FreeLine_Point]  WITH CHECK ADD  CONSTRAINT [FK_FreeLine_Point_Point] FOREIGN KEY([PointID])
REFERENCES [dbo].[Point] ([PointID])

ALTER TABLE [dbo].[FreeLine_Point] CHECK CONSTRAINT [FK_FreeLine_Point_Point]

ALTER TABLE [dbo].[Line]  WITH CHECK ADD  CONSTRAINT [FK_Line_Drawing] FOREIGN KEY([PictureID])
REFERENCES [dbo].[Drawing] ([Drawing_ID])
ON DELETE CASCADE

ALTER TABLE [dbo].[Line] CHECK CONSTRAINT [FK_Line_Drawing]

ALTER TABLE [dbo].[Rectangle]  WITH CHECK ADD  CONSTRAINT [FK_Rectangle_Drawing] FOREIGN KEY([PictureID])
REFERENCES [dbo].[Drawing] ([Drawing_ID])
ON DELETE CASCADE

ALTER TABLE [dbo].[Rectangle] CHECK CONSTRAINT [FK_Rectangle_Drawing]

USE [master]

ALTER DATABASE [819523465] SET  READ_WRITE 



