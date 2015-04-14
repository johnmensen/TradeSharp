<?
//+------------------------------------------------------------------+
//| WebRegistration: Configuration                                   |
//+------------------------------------------------------------------+
define('T_PAGE_ENCODING', 'windows-1251');            // Web: Page encoding

define('T_SMTP_SERVER',   'mail.mycompany.com');      // SMTP: Server
define('T_SMTP_LOGIN',    'login');                   // SMTP: Login
define('T_SMTP_PASSWORD', 'password');                // SMTP: Password
define('T_SMTP_FROM',     'hello@mycompany.com');     // SMTP: From
define('T_SMTP_FROM_NAME','Mr. Hello');               // SMTP: From Name

define('T_MT4_HOST',      '127.0.0.1');               // MetaTrader: Server
define('T_MT4_PORT',       443);                      // MetaTrader: Port

define('T_PLUGIN_MASTER', 'master');                  // Plugin: Master Password

define('T_MYSQL_SERVER',  '192.168.0.1');             // MySQL: Server
define('T_MYSQL_LOGIN',   'mysql_login');             // MySQL: Login
define('T_MYSQL_PASSWORD','mysql_password');          // MySQL: Password
define('T_MYSQL_DB_NAME', 'webregistration');         // MySQL: Database
//+------------------------------------------------------------------+
//| Group mapping                                                    |
//+------------------------------------------------------------------+
function Group($group)
  {
   switch($group)
    {
     case 'demoforex':     $ret=1; break;
     case 'demoforex-usd': $ret=2; break;
     case 'demoforex-eur': $ret=3; break;
     case 'demoforex-jpy': $ret=4; break;
     default:              $ret=0; break;
    }
   return($ret);
  }
//+------------------------------------------------------------------+
?>