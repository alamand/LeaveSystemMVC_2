USE [master]
GO
CREATE DATABASE [LeaveSystem]
GO
ALTER DATABASE [LeaveSystem] SET COMPATIBILITY_LEVEL = 120
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employee](
	[Employee_ID] [int] NOT NULL,
	[First_Name] [varchar](25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Last_Name] [varchar](25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[User_Name] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Password] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Designation] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Email] [varchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Gender] [varchar](1) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Department_ID] [int] NULL,
	[Ph_No] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Account_Status] [bit] NOT NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Employee_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE TABLE [dbo].[Department](
	[Department_ID] [int] IDENTITY(1,1) NOT NULL,
	[Department_Name] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Line_Manager_ID] [int] NOT NULL,
	[Substitute_LM_ID] [int] NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Department_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE TABLE [dbo].[DaysInLieu](
	[Employee_ID] [int] NOT NULL,
	[DIL_Date] [nchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[NumDays] [decimal](3, 1) NOT NULL,
	[Comment] [varchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_DaysInLieu] PRIMARY KEY CLUSTERED 
(
	[Employee_ID] ASC,
	[DIL_Date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (10101, N'Dan', N'Adkins', N'dan.adkins', N'dan', N'CEO', N'dan.adkins@aol.com', N'M', NULL, N'023456789', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (11111, N'Admin', N'Admin', N'admin', N'admin', N'Superuser', N'admin@murdoch.com', N'M', NULL, N'043245678', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (12121, N'Liz', N'Pool', N'liz.pool', N'liz', N'MDFP2', N'mandy.northover@aol.co.za', N'M', 4, N'00971566093333', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (12333, N'Julz', N'Dev', N'Julz', N'G$a3?6T', N'IT', N'jul@lz.com', N'F', 3, N'042131232', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (22222, N'Suchi', N'Sharma', N'suchi.sharma', N'suchi', N'HR responsible', N'suchi.sharma@murdoch.com', N'F', 5, N'0566094444', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (33120, N'sam', N'Lev', N'sam.lev', N'7i=LZ*4', N'Finance', N'sam@lev.com', N'M', 4, N'042131222', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (33333, N'Mandy', N'Northover', N'mandy.northover', N'mandy', N'Lecturer', N'mandy.northover@murdochdubai.ac.ae', N'M', 6, N'0566394446', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (44444, N'Biju', N'Veetil', N'biju.veetil', N'biju', N'IT head', N'biju.veetil@aol.com', N'M', 3, N'066445555', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (55444, N'Hamed', N'Alhinai', N'Ham', N'Fs6+2W&', N'IT', N'ham@do.com', N'M', 3, N'042238222', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (55555, N'Wayne', N'Muller', N'wayne.muller', N'wayne', N'Academic Director', N'wayne.muller@murdoch.com', N'M', 6, N'043239875', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (55655, N'John', N'Doe', N'john.doe', N'jD!9%k7', N'', N'john.doe@doe.com', N'M', 4, N'066445522', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (66666, N'Manish', N'Chetwani', N'manish.chetwani', N'manish', N'Finance', N'manish.chetwani@google.com', N'M', 4, N'0566034445', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (77777, N'Pratima', N'Joshi', N'pratima.joshi', N'pratima', N'Student Affairs', N'pratima.joshi@aol.com', N'F', 1, N'0566093445', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (88888, N'Tracie', N'Scott', N'tracie.scott', N'tracie', N'Lecturer', N'mandy.northover@murdochdubai.ac.ae', N'F', 6, N'0566394446', 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee] ([Employee_ID], [First_Name], [Last_Name], [User_Name], [Password], [Designation], [Email], [Gender], [Department_ID], [Ph_No], [Account_Status], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (99999, N'Krystle', N'Vaz', N'krystle.vaz', N'krystle', N'HR', N'krystle@aol.com', N'F', 5, N'043267890', 1, NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Department] ON 

GO
INSERT [dbo].[Department] ([Department_ID], [Department_Name], [Line_Manager_ID], [Substitute_LM_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (1, N'Student Affairs', 77777, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Department] ([Department_ID], [Department_Name], [Line_Manager_ID], [Substitute_LM_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (3, N'IT', 44444, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Department] ([Department_ID], [Department_Name], [Line_Manager_ID], [Substitute_LM_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (4, N'Finance', 66666, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Department] ([Department_ID], [Department_Name], [Line_Manager_ID], [Substitute_LM_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (5, N'HR', 22222, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Department] ([Department_ID], [Department_Name], [Line_Manager_ID], [Substitute_LM_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (6, N'Academic Services', 55555, NULL, NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Department] OFF
GO
ALTER TABLE [dbo].[Employee]  WITH CHECK ADD FOREIGN KEY([Department_ID])
REFERENCES [dbo].[Department] ([Department_ID])
GO
ALTER TABLE [dbo].[Department]  WITH CHECK ADD FOREIGN KEY([Line_Manager_ID])
REFERENCES [dbo].[Employee] ([Employee_ID])
GO
ALTER TABLE [dbo].[Department]  WITH CHECK ADD FOREIGN KEY([Substitute_LM_ID])
REFERENCES [dbo].[Employee] ([Employee_ID])
GO
ALTER TABLE [dbo].[DaysInLieu]  WITH NOCHECK ADD  CONSTRAINT [FK_DaysInLieu_Employee] FOREIGN KEY([Employee_ID])
REFERENCES [dbo].[Employee] ([Employee_ID])
GO
ALTER TABLE [dbo].[DaysInLieu] CHECK CONSTRAINT [FK_DaysInLieu_Employee]
GO
CREATE TABLE [dbo].[Employee_Role](
	[Emp_Role_ID] [int] IDENTITY(1,1) NOT NULL,
	[Employee_ID] [int] NOT NULL,
	[Role_ID] [int] NOT NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Emp_Role_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE TABLE [dbo].[Role](
	[Role_ID] [int] IDENTITY(1,1) NOT NULL,
	[Role_Name] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Role_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[Employee_Role] ON 

GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (85, 44444, 4, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (86, 44444, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (87, 22222, 2, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (89, 66666, 4, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (90, 66666, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (91, 22222, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (92, 11111, 1, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (93, 55555, 4, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (94, 55555, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (95, 77777, 4, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (96, 77777, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (97, 99999, 3, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (98, 99999, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (100, 88888, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (102, 10101, 4, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (106, 33333, 4, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (107, 33333, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (116, 12333, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (117, 55444, 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employee_Role] ([Emp_Role_ID], [Employee_ID], [Role_ID], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (118, 12121, 5, NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Employee_Role] OFF
GO
SET IDENTITY_INSERT [dbo].[Role] ON 

GO
INSERT [dbo].[Role] ([Role_ID], [Role_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (1, N'Admin', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Role] ([Role_ID], [Role_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (2, N'HR_Responsible', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Role] ([Role_ID], [Role_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (3, N'HR', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Role] ([Role_ID], [Role_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (4, N'LM', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Role] ([Role_ID], [Role_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (5, N'Staff', NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Role] OFF
GO
ALTER TABLE [dbo].[Employee_Role]  WITH CHECK ADD FOREIGN KEY([Employee_ID])
REFERENCES [dbo].[Employee] ([Employee_ID])
GO
ALTER TABLE [dbo].[Employee_Role]  WITH CHECK ADD FOREIGN KEY([Role_ID])
REFERENCES [dbo].[Role] ([Role_ID])
GO
CREATE TABLE [dbo].[Employment_Period](
	[Employee_ID] [int] NOT NULL,
	[Emp_Start_Date] [date] NOT NULL,
	[Emp_End_Date] [date] NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Employment_Period] PRIMARY KEY CLUSTERED 
(
	[Employee_ID] ASC,
	[Emp_Start_Date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
INSERT [dbo].[Employment_Period] ([Employee_ID], [Emp_Start_Date], [Emp_End_Date], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (12121, CAST(N'2017-11-01' AS Date), CAST(N'2017-12-01' AS Date), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employment_Period] ([Employee_ID], [Emp_Start_Date], [Emp_End_Date], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (12333, CAST(N'2017-11-19' AS Date), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Employment_Period] ([Employee_ID], [Emp_Start_Date], [Emp_End_Date], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (55444, CAST(N'2017-11-21' AS Date), NULL, NULL, NULL, NULL, NULL, NULL)
GO
ALTER TABLE [dbo].[Employment_Period]  WITH NOCHECK ADD  CONSTRAINT [FK_Employment_Period_Employee] FOREIGN KEY([Employee_ID])
REFERENCES [dbo].[Employee] ([Employee_ID])
GO
ALTER TABLE [dbo].[Employment_Period] CHECK CONSTRAINT [FK_Employment_Period_Employee]
GO
CREATE TABLE [dbo].[Leave_Type](
	[Leave_ID] [int] IDENTITY(1,1) NOT NULL,
	[Leave_Name] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Duration] [int] NOT NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Leave_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE TABLE [dbo].[Leave](
	[Leave_Application_ID] [int] IDENTITY(1,1) NOT NULL,
	[Employee_ID] [int] NOT NULL,
	[Start_Date] [date] NOT NULL,
	[End_Date] [date] NOT NULL,
	[Reporting_Back_Date] [date] NULL,
	[Leave_ID] [int] NOT NULL,
	[Contact_Outside_UAE] [varchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Comment] [varchar](300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Document] [varchar](300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Flight_Ticket] [bit] NULL,
	[Total_Leave_Days] [int] NOT NULL,
	[Start_Hrs] [time](7) NULL,
	[End_Hrs] [time](7) NULL,
	[Leave_Status_ID] [int] NOT NULL,
	[LM_Comment] [varchar](300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[HR_Comment] [varchar](300) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Leave_Application_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE TABLE [dbo].[Leave_Status](
	[Leave_Status_ID] [int] IDENTITY(0,1) NOT NULL,
	[Status_Name] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Leave_Status] PRIMARY KEY CLUSTERED 
(
	[Leave_Status_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[Leave_Type] ON 

GO
INSERT [dbo].[Leave_Type] ([Leave_ID], [Leave_Name], [Duration], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (1, N'Annual', 22, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Type] ([Leave_ID], [Leave_Name], [Duration], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (2, N'Maternity', 40, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Type] ([Leave_ID], [Leave_Name], [Duration], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (3, N'Sick', 5, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Type] ([Leave_ID], [Leave_Name], [Duration], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (4, N'Compassionate', 3, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Type] ([Leave_ID], [Leave_Name], [Duration], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (5, N'DIL', 0, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Type] ([Leave_ID], [Leave_Name], [Duration], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (6, N'Short_Hours', 6, NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Leave_Type] OFF
GO
SET IDENTITY_INSERT [dbo].[Leave] ON 

GO
INSERT [dbo].[Leave] ([Leave_Application_ID], [Employee_ID], [Start_Date], [End_Date], [Reporting_Back_Date], [Leave_ID], [Contact_Outside_UAE], [Comment], [Document], [Flight_Ticket], [Total_Leave_Days], [Start_Hrs], [End_Hrs], [Leave_Status_ID], [LM_Comment], [HR_Comment], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (25, 12121, CAST(N'2017-11-15' AS Date), CAST(N'2017-11-16' AS Date), CAST(N'2017-11-16' AS Date), 3, N'', N'', N'', 0, 1, CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 5, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave] ([Leave_Application_ID], [Employee_ID], [Start_Date], [End_Date], [Reporting_Back_Date], [Leave_ID], [Contact_Outside_UAE], [Comment], [Document], [Flight_Ticket], [Total_Leave_Days], [Start_Hrs], [End_Hrs], [Leave_Status_ID], [LM_Comment], [HR_Comment], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (26, 12121, CAST(N'2017-11-15' AS Date), CAST(N'2017-11-16' AS Date), CAST(N'2017-11-16' AS Date), 3, N'', N'', N'', 0, 1, CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 4, N'', NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave] ([Leave_Application_ID], [Employee_ID], [Start_Date], [End_Date], [Reporting_Back_Date], [Leave_ID], [Contact_Outside_UAE], [Comment], [Document], [Flight_Ticket], [Total_Leave_Days], [Start_Hrs], [End_Hrs], [Leave_Status_ID], [LM_Comment], [HR_Comment], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (27, 44444, CAST(N'2017-11-19' AS Date), CAST(N'2017-11-20' AS Date), CAST(N'2017-11-20' AS Date), 1, N'+97147614444', N'Home', N'', 0, 1, CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 5, N'', NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave] ([Leave_Application_ID], [Employee_ID], [Start_Date], [End_Date], [Reporting_Back_Date], [Leave_ID], [Contact_Outside_UAE], [Comment], [Document], [Flight_Ticket], [Total_Leave_Days], [Start_Hrs], [End_Hrs], [Leave_Status_ID], [LM_Comment], [HR_Comment], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (28, 44444, CAST(N'2017-11-22' AS Date), CAST(N'2017-11-23' AS Date), CAST(N'2017-11-23' AS Date), 1, N'+97147614444', N'Home', N'', 0, 1, CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 2, N'', NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave] ([Leave_Application_ID], [Employee_ID], [Start_Date], [End_Date], [Reporting_Back_Date], [Leave_ID], [Contact_Outside_UAE], [Comment], [Document], [Flight_Ticket], [Total_Leave_Days], [Start_Hrs], [End_Hrs], [Leave_Status_ID], [LM_Comment], [HR_Comment], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (29, 44444, CAST(N'2017-11-24' AS Date), CAST(N'2017-11-25' AS Date), CAST(N'2017-11-25' AS Date), 2, N'+97147614444', N'Home', N'', 0, 1, CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 5, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave] ([Leave_Application_ID], [Employee_ID], [Start_Date], [End_Date], [Reporting_Back_Date], [Leave_ID], [Contact_Outside_UAE], [Comment], [Document], [Flight_Ticket], [Total_Leave_Days], [Start_Hrs], [End_Hrs], [Leave_Status_ID], [LM_Comment], [HR_Comment], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (30, 22222, CAST(N'2017-11-20' AS Date), CAST(N'2017-11-21' AS Date), CAST(N'2017-11-21' AS Date), 4, N'+96899020244', N'Work', N'', 0, 1, CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 5, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave] ([Leave_Application_ID], [Employee_ID], [Start_Date], [End_Date], [Reporting_Back_Date], [Leave_ID], [Contact_Outside_UAE], [Comment], [Document], [Flight_Ticket], [Total_Leave_Days], [Start_Hrs], [End_Hrs], [Leave_Status_ID], [LM_Comment], [HR_Comment], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (31, 33333, CAST(N'2017-11-28' AS Date), CAST(N'2017-11-29' AS Date), CAST(N'2017-11-29' AS Date), 3, N'+97147614444', N'SICK', N'', 0, 1, CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 5, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave] ([Leave_Application_ID], [Employee_ID], [Start_Date], [End_Date], [Reporting_Back_Date], [Leave_ID], [Contact_Outside_UAE], [Comment], [Document], [Flight_Ticket], [Total_Leave_Days], [Start_Hrs], [End_Hrs], [Leave_Status_ID], [LM_Comment], [HR_Comment], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (32, 44444, CAST(N'2017-11-28' AS Date), CAST(N'2017-11-30' AS Date), CAST(N'2017-11-30' AS Date), 1, N'+97147614444', N'Travel', N'', 0, 2, CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), 5, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Leave] OFF
GO
SET IDENTITY_INSERT [dbo].[Leave_Status] ON 

GO
INSERT [dbo].[Leave_Status] ([Leave_Status_ID], [Status_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (0, N'Pending_LM', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Status] ([Leave_Status_ID], [Status_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (1, N'Pending_HR', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Status] ([Leave_Status_ID], [Status_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (2, N'Approved', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Status] ([Leave_Status_ID], [Status_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (3, N'Rejected_LM', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Status] ([Leave_Status_ID], [Status_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (4, N'Rejected_HR', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Status] ([Leave_Status_ID], [Status_Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (5, N'Cancelled', NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Leave_Status] OFF
GO
ALTER TABLE [dbo].[Leave]  WITH CHECK ADD  CONSTRAINT [FK_Leave_Employee] FOREIGN KEY([Employee_ID])
REFERENCES [dbo].[Employee] ([Employee_ID])
GO
ALTER TABLE [dbo].[Leave] CHECK CONSTRAINT [FK_Leave_Employee]
GO
ALTER TABLE [dbo].[Leave]  WITH CHECK ADD  CONSTRAINT [FK_Leave_Leave_Type] FOREIGN KEY([Leave_ID])
REFERENCES [dbo].[Leave_Type] ([Leave_ID])
GO
ALTER TABLE [dbo].[Leave] CHECK CONSTRAINT [FK_Leave_Leave_Type]
GO
ALTER TABLE [dbo].[Leave]  WITH NOCHECK ADD  CONSTRAINT [FK_Leave_Status] FOREIGN KEY([Leave_Status_ID])
REFERENCES [dbo].[Leave_Status] ([Leave_Status_ID])
GO
ALTER TABLE [dbo].[Leave] CHECK CONSTRAINT [FK_Leave_Status]
GO
CREATE TABLE [dbo].[Leave_Balance](
	[Leave_Balance_ID] [int] IDENTITY(1,1) NOT NULL,
	[Employee_ID] [int] NOT NULL,
	[Leave_ID] [int] NOT NULL,
	[Balance] [decimal](4, 1) NOT NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_Leave_Balance] PRIMARY KEY CLUSTERED 
(
	[Leave_Balance_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[Leave_Balance] ON 

GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (7, 12121, 1, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (8, 12121, 2, CAST(40.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (9, 12121, 3, CAST(5.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (10, 12121, 5, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (11, 12121, 4, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (12, 12121, 6, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (13, 55555, 1, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (14, 55555, 2, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (15, 55555, 3, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (16, 55555, 5, CAST(1.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (17, 55555, 4, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (18, 55555, 6, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (19, 10101, 1, CAST(4.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (20, 10101, 2, CAST(4.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (21, 10101, 3, CAST(4.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (22, 10101, 5, CAST(4.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (23, 10101, 4, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (24, 10101, 6, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (25, 55444, 1, CAST(2.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (26, 55444, 2, CAST(4.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (27, 55444, 3, CAST(2.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (28, 55444, 5, CAST(8.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (29, 55444, 4, CAST(6.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (30, 55444, 6, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (31, 22222, 1, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (32, 22222, 2, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (33, 22222, 3, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (34, 22222, 5, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (35, 22222, 4, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Leave_Balance] ([Leave_Balance_ID], [Employee_ID], [Leave_ID], [Balance], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (36, 22222, 6, CAST(0.0 AS Decimal(4, 1)), NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Leave_Balance] OFF
GO
ALTER TABLE [dbo].[Leave_Balance]  WITH NOCHECK ADD  CONSTRAINT [FK_Leave_Balance_Employee] FOREIGN KEY([Employee_ID])
REFERENCES [dbo].[Employee] ([Employee_ID])
GO
ALTER TABLE [dbo].[Leave_Balance] CHECK CONSTRAINT [FK_Leave_Balance_Employee]
GO
ALTER TABLE [dbo].[Leave_Balance]  WITH NOCHECK ADD  CONSTRAINT [FK_Leave_Balance_Leave] FOREIGN KEY([Leave_ID])
REFERENCES [dbo].[Leave_Type] ([Leave_ID])
GO
ALTER TABLE [dbo].[Leave_Balance] CHECK CONSTRAINT [FK_Leave_Balance_Leave]
GO
CREATE TABLE [dbo].[Public_Holiday](
	[Public_Holiday_ID] [int] IDENTITY(1,1) NOT NULL,
	[Date] [date] NOT NULL,
	[Name] [varchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Created_On] [datetime] NULL,
	[Modified_By] [varchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[Modified_On] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Public_Holiday_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[Public_Holiday] ON 

GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (4, CAST(N'2017-12-01' AS Date), N'National Day', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (5, CAST(N'2017-12-02' AS Date), N'National Day', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (8, CAST(N'2017-06-24' AS Date), N'Eid Al Fitr', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (9, CAST(N'2017-06-25' AS Date), N'Eid Al Fitr', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (10, CAST(N'2017-06-26' AS Date), N'Eid Al Fitr', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (11, CAST(N'2017-08-31' AS Date), N'Arafat day', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (12, CAST(N'2017-09-01' AS Date), N'Eid Al Adha', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (13, CAST(N'2017-09-02' AS Date), N'Eid Al Adha', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (14, CAST(N'2017-09-21' AS Date), N'Islamic New Year', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (15, CAST(N'2017-09-30' AS Date), N'Martyrs'' day', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (17, CAST(N'2017-01-01' AS Date), N'New Year''s Day', NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Public_Holiday] ([Public_Holiday_ID], [Date], [Name], [Created_By], [Created_On], [Modified_By], [Modified_On], [IsActive]) VALUES (19, CAST(N'2017-08-22' AS Date), N'Mandy''s day', NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Public_Holiday] OFF
GO
USE [master]
GO
ALTER DATABASE [LeaveSystem] SET READ_WRITE
GO
