---------------------------------------------------------------------------------------------
-- Функция - возвращает имя пользователя в удобочитаемом формате
---------------------------------------------------------------------------------------------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION MakeUserNameWithInitials
(
	@Login varchar(25),
	@Name varchar(50),
	@Surname varchar(50),	
	@Patronym varchar(50)
)
RETURNS varchar(56)
AS
BEGIN
	declare @nameUser varchar(56)
    --if (string.IsNullOrEmpty(Surname)) return string.IsNullOrEmpty(Name) ? Login : Name;
	if @Surname is NULL or @Surname = ''
	begin
	  if @Name is NULL or @Name = ''
	    set @nameUser = @Login
	  else
	    set @nameUser = @Name
	  return @nameUser
	end
	-- if (string.IsNullOrEmpty(Name)) return string.IsNullOrEmpty(Surname) ? Login : Surname;
	if @Name is NULL or @Name = ''
	begin
	  if @Surname is NULL or @Surname = ''
	    set @nameUser = @Login
	  else
	    set @nameUser = @Surname
	  return @nameUser
	end
	
	--            return string.IsNullOrEmpty(Patronym)
    --                ? string.Format("{0} {1}.", Surname, Name[0])
    --                : string.Format("{0} {1}. {2}.", Surname, Name[0], Patronym[0]);
	if @Patronym is NULL or @Patronym = ''
	  set @nameUser = @Surname + ' ' + SUBSTRING(@Name, 1, 1) + '.'
	else
	  set @nameUser = @Surname + ' ' + SUBSTRING(@Name, 1, 1) + '. ' + SUBSTRING(@Patronym, 1, 1) + '.'
	return @nameUser
END
GO

---------------------------------------------------------------------------------------------
-- Процедура - возвращает все счета вместе с именами пользователей - владельцев счетов
---------------------------------------------------------------------------------------------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE GetAllAccounts	
AS
BEGIN
	SET NOCOUNT ON;

	select ID, AccountGroup, Currency, Balance, 
	  STUFF(
	   (select ',' + 
	      dbo.MakeUserNameWithInitials(usr.Login, usr.Name, usr.Surname, usr.Patronym)
	    from PLATFORM_USER usr join PLATFORM_USER_ACCOUNT ua
	      on usr.ID = ua.PlatformUser
	    where ac.ID = ua.Account for xml path('')), 1, 1, '') UserNames
	from ACCOUNT ac    
END
GO

---------------------------------------------------------------------------------------------
-- Процедура возвращает все счета вместе с именами и Id пользователей - владельцев счетов
---------------------------------------------------------------------------------------------
Create PROCEDURE [dbo].[GetAllAccountsUserDetail]	
AS
BEGIN
	SET NOCOUNT ON;

	select ID, AccountGroup, Currency, Balance, 
	  STUFF(
	   (select ',' + 
	      dbo.MakeUserNameWithInitials(usr.Login, usr.Name, usr.Surname, usr.Patronym)
	    from PLATFORM_USER usr join PLATFORM_USER_ACCOUNT ua
	      on usr.ID = ua.PlatformUser
	    where ac.ID = ua.Account for xml path('')), 1, 1, '') UserNames,
	    STUFF(
	   (select ',' + 
	      convert(VARCHAR, usr.ID)
	    from PLATFORM_USER usr join PLATFORM_USER_ACCOUNT ua
	      on usr.ID = ua.PlatformUser
	    where ac.ID = ua.Account for xml path('')), 1, 1, '') UserId
	from ACCOUNT ac    
END

---------------------------------------------------------------------------------------------
-- Процедура возвращает вводы - выводы средств по депозитам, не учитывая результаты сделок
---------------------------------------------------------------------------------------------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE GetDepoTransfers
AS
BEGIN	
	SET NOCOUNT ON;

	select 
	  bc.*, a.Currency, gr.Code, gr.IsReal  
	from 
    BALANCE_CHANGE bc 
      join ACCOUNT a on bc.AccountID = a.ID
      join ACCOUNT_GROUP gr on a.AccountGroup = gr.Code
    where bc.ChangeType = 1 or bc.ChangeType = 2  
END
GO

---------------------------------------------------------------------------------------------
-- Процедура возвращает билли по счетам с указанием валюты счета и типа (демо - риал)
---------------------------------------------------------------------------------------------

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE GetOrderBills
AS
BEGIN	
	SET NOCOUNT ON;
	
select b.*, a.AccountGroup, a.Currency, g.IsReal from ORDER_BILL b
  left join POSITION p on b.Position = p.ID
  left join POSITION_CLOSED pc on b.Position = pc.ID
  join ACCOUNT a on a.ID = p.AccountID or a.ID = pc.AccountID
  join ACCOUNT_GROUP g on a.AccountGroup = g.Code
  
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

---------------------------------------------------------------------------------------------
-- Процедура возвращает группы с количеством счетов по каждой
---------------------------------------------------------------------------------------------
USE [MTS_LIVE]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetGroupsWithAccounts]
(@AccountGroupCode varchar(50))
AS
BEGIN	
	SET NOCOUNT ON;

		select ag.*, (select COUNT(*) from ACCOUNT where AccountGroup = Code) as Accounts,
		(select Dealer from DEALER_GROUP where ag.Code = DEALER_GROUP.AccountGroup) as DealerCode,
(select [FileName] from DEALER where 
	DEALER.Code = (select Dealer from DEALER_GROUP where ag.Code = DEALER_GROUP.AccountGroup)) as [FileName],
(select DealerEnabled from DEALER where 
	DEALER.Code = (select Dealer from DEALER_GROUP where ag.Code = DEALER_GROUP.AccountGroup)) as DealerEnabled,	
(select SessionName from DEALER_GROUP where ag.Code = DEALER_GROUP.AccountGroup) as SessionName,	
(select MessageQueue from DEALER_GROUP where ag.Code = DEALER_GROUP.AccountGroup) as MessageQueue,
(select HedgingAccount from DEALER_GROUP where ag.Code = DEALER_GROUP.AccountGroup) as HedgingAccount,	
(select SenderCompId from DEALER_GROUP where ag.Code = DEALER_GROUP.AccountGroup) as SenderCompId
		from ACCOUNT_GROUP ag where @AccountGroupCode is NULL or ag.Code = @AccountGroupCode 
END
GO


---------------------------------------------------------------------------------------------
-- Процедура возвращает счета, которые появятся в списке управляющих
---------------------------------------------------------------------------------------------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE GetPerformers
AS
BEGIN	
	SET NOCOUNT ON;

a.ID as 'Account', a.Currency, 
( select Signal from ACCOUNT_TRADE_SIGNAL where Account = a.ID and (RightsMask = 2 or RightsMask = 6 or RightsMask = 7) ) as 'Signal',
( select c.Title from ACCOUNT_TRADE_SIGNAL ts join TRADE_SIGNAL_CATEGORY c on ts.Signal = c.Id 
  where Account = a.ID and (RightsMask = 2 or RightsMask = 6) ) as 'Title', 
 a.AccountGroup,
 (select COUNT(*) from ACCOUNT_TRADE_SIGNAL where Account = a.ID) as 'SubscriberCount', s.[User] as 'PlatformUser',
 STUFF(
	   (select ',' + 
	      dbo.MakeUserNameWithInitials(usr.Login, usr.Name, usr.Surname, usr.Patronym)
	    from PLATFORM_USER usr join PLATFORM_USER_ACCOUNT ua
	      on usr.ID = ua.PlatformUser
	    where a.ID = ua.Account for xml path('')), 1, 1, '') as 'UserNames',
	    s.FixedPrice
from 
 ACCOUNT a 
 join [SERVICE] s on s.AccountId = a.ID
 
END

---------------------------------------------------------------------------------------------
-- Процедура - возвращает все открытые позы для групп не SwapFree
---------------------------------------------------------------------------------------------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE GetPositionsToSwap
AS
BEGIN
	SET NOCOUNT ON;

	select p.*, a.Currency
	from POSITION p join ACCOUNT a on p.AccountID = a.ID
      join ACCOUNT_GROUP g on a.AccountGroup = g.Code
    where g.SwapFree = 0

END
GO


---------------------------------------------------------------------------------------------
-- Процедура возвращает все позиции, отфильтроканный указанным образом по 8 фильтрам
---------------------------------------------------------------------------------------------
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE GetPositionList
(
    @CountItemShow int, 
    @AccountId int,
    @IsDemoAccount int,
    @Symbol varchar(12),
    @Status int,
    @Side int,
    @TimeOpenFrom datetime,
    @TimeOpenTo datetime,
    @TimeExitFrom datetime,
    @TimeExitTo datetime,
    @TotalItemCount int OUTPUT
)
AS
    BEGIN
    
    DECLARE @TempTable TABLE
	(
		ID int not null,
		IsClosed int not null,
		Symbol varchar(12) not null,
		AccountID int,
		PriceEnter decimal(8, 5) not null,
		PriceExit decimal(8, 5),
		TimeEnter datetime not null,
		TimeExit datetime,
		Side int not null,
		ResultPoints decimal(8, 2),
		ResultDepo decimal(16, 2)
	);
	
	DECLARE @AllDataTable TABLE
	(
		ID int not null,
		IsClosed int not null,
		Symbol varchar(12) not null,
		AccountID int,
		PriceEnter decimal(8, 5) not null,
		PriceExit decimal(8, 5),
		TimeEnter datetime not null,
		TimeExit datetime,
		Side int not null,
		ResultPoints decimal(8, 2),
		ResultDepo decimal(16, 2)
	);

	insert into @AllDataTable
        Select ID, IsClosed = 0, Symbol, AccountID, PriceEnter, PriceExit = null, TimeEnter, TimeExit = null, Side, ResultPoints = null, ResultDepo = null from POSITION

	insert into @AllDataTable
        Select ID, IsClosed = 1, Symbol, AccountID, PriceEnter, PriceExit, TimeEnter, TimeExit, Side, ResultPoints, ResultDepo from POSITION_CLOSED
        
        
		--Фильтр по счёту
        if (@AccountId IS NOT NULL)
        begin
			insert into @TempTable
			Select * from @AllDataTable t
			where t.AccountID = @AccountId
        end   
        Else
		insert into @TempTable
			Select * from @AllDataTable
						
        Delete FROM @AllDataTable
        insert into @AllDataTable Select * from @TempTable
		Delete FROM @TempTable
		
		--фильтр по статусу
	    if (@Status IS NOT NULL)
        begin
			insert into @TempTable
			Select * from @AllDataTable t
			where t.IsClosed = @Status
        end
		Else
		insert into @TempTable
			Select * from @AllDataTable		
		
		Delete FROM @AllDataTable
        insert into @AllDataTable Select * from @TempTable
		Delete FROM @TempTable
		
		-- фильтр по типу
	    if (@Side IS NOT NULL)
        begin
			insert into @TempTable
			Select * from @AllDataTable t
			where t.Side = @Side
        end
		Else
		insert into @TempTable
			Select * from @AllDataTable
				
		Delete FROM @AllDataTable
        insert into @AllDataTable Select * from @TempTable
		Delete FROM @TempTable		
		
		-- фильтр по активу
		if (@Symbol IS NOT NULL)
        begin
			insert into @TempTable
			Select * from @AllDataTable t
			where t.Symbol = @Symbol
        end
		Else
		insert into @TempTable
			Select * from @AllDataTable
			
		Delete FROM @AllDataTable
        insert into @AllDataTable Select * from @TempTable
		Delete FROM @TempTable		
			
		-- фильтр по дате входа	
		if (@TimeOpenFrom IS NOT NULL AND @TimeOpenTo IS NOT NULL)
        begin
			insert into @TempTable
			Select * from @AllDataTable t
			where t.TimeEnter >= @TimeOpenFrom and t.TimeEnter <= @TimeOpenTo
        end
		Else
		insert into @TempTable
			Select * from @AllDataTable
			
		Delete FROM @AllDataTable
        insert into @AllDataTable Select * from @TempTable
		Delete FROM @TempTable
		
		-- Фильтр по дате выхода	
		if (@TimeExitFrom IS NOT NULL AND @TimeExitTo IS NOT NULL)
        begin
			insert into @TempTable
			Select * from @AllDataTable t
			where t.TimeExit >= @TimeExitFrom and t.TimeExit <= @TimeExitTo
        end
		Else
		insert into @TempTable
			Select * from @AllDataTable
						
		Delete FROM @AllDataTable
        insert into @AllDataTable Select * from @TempTable
		Delete FROM @TempTable
			
		-- фильтр по типу счёта (реальная / демо)
		if (@IsDemoAccount IS NOT NULL)
        begin
			insert into @TempTable
			Select t.ID, t.IsClosed, t.Symbol, t.AccountID, t.PriceEnter, t.PriceExit, t.TimeEnter, t.TimeExit, t.Side, t.ResultPoints, t.ResultDepo 
			from @AllDataTable t join ACCOUNT a on t.AccountID = a.ID join ACCOUNT_GROUP g on a.AccountGroup = g.Code
		    where g.IsReal = @IsDemoAccount
        end
		Else
		insert into @TempTable
			Select * from @AllDataTable
		   
		SET @TotalItemCount = (select COUNT (*) from  @TempTable) 
        select top(@CountItemShow) * from @TempTable 
    END