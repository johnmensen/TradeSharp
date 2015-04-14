<?php
include_once("wr_cfg.php");
//+------------------------------------------------------------------+
//| WebRegistration: Web-Services API                                |
//+------------------------------------------------------------------+
function MQ_Query($query)
  {
   $ret='error';
//---- open socket
   $ptr=@fsockopen(T_MT4_HOST,T_MT4_PORT,$errno,$errstr,5); 
//---- check connection
   if($ptr)
     {
      //---- send request
      if(fputs($ptr,"W$query\nQUIT\n")!=FALSE)
        {
         //---- clear default answer
         $ret='';
         //---- receive answer
         while(!feof($ptr)) 
          {
           $line=fgets($ptr,128);
           if($line=="end\r\n") break; 
           $ret.= $line;
          } 
        }
      fclose($ptr);
     }
//---- return answer
   return $ret;

  }
//+------------------------------------------------------------------+
?>