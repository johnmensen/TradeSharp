USE fxi_quote
CREATE TABLE NEWS 
(
	Id	INT PRIMARY KEY NOT NULL IDENTITY (1,1),
	Channel INT NOT NULL,
	DateNews	DATETIME NOT NULL,
	Title	VARCHAR(256) NOT NULL,
	Body	VARCHAR(MAX) NULL
)
GO

CREATE PROCEDURE FindQuote
(
    @ticker int,
	@date datetime	
)
AS
BEGIN
 select top(1) * from QUOTE 
	where ticker = @ticker and date <= @date
	order by date desc
END
GO

CREATE PROCEDURE GetLastQuote
(
    @ticker int	
)
AS
BEGIN
 select top(1) * from QUOTE 
	where ticker = @ticker
	order by date desc
END