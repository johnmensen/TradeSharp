<?
include_once("wr_mq.php");
include_once("wr_tools.php");
include_once("wr_cfg.php");  
//+------------------------------------------------------------------+
//| WebRegistration: Registration Form                               |
//+------------------------------------------------------------------+
class CWebForm
  {
   var $crypt_key='682d95a2009e19fb3570ccb4d98b820f+аривпол';
   var $user;
   var $verify_code;

   function __construct()
     {
      //---- start session
      session_start();
      //---- initialize
      $this->user=array('email'           =>'' ,'name'            =>'',
                        'password'        =>'' ,'country'         =>'',
                        'state'           =>'' ,'city'            =>'',
                        'zipcode'         =>'' ,'phone'           =>'',
                        'phone_password'  =>'' ,'address'         =>'',
                        'password'        =>'' ,'confirm_password'=>'',
                        'send_reports'    =>'0','deposit'         =>'0');
     }

   function Show()
     {
      //--- set verify code
      $_SESSION['verify_code']=strtoupper(substr(uniqid(''),-6));
      //--- show page
      include('wr_register.phtml');
     }

   
   function OnRegister()
     {
      global $root;
      //---- validation request
      if(!isset($_REQUEST['user'])        || !is_array($_REQUEST['user'])    ||
         !isset($_REQUEST['verify_code']) || empty($_REQUEST['verify_code']) ||
         !isset($_SESSION['verify_code']) || empty($_SESSION['verify_code']))
        {
         header("Location: http://$root/wr_error_invalid.html");
         exit;
        }
      //---- receive input
      $this->user       =$_REQUEST['user'];
      $this->verify_code=$_REQUEST['verify_code'];
      //---- 
      foreach($this->user as $key=>$value) $value=trim($value);
      //---- validation input
      if($_SESSION['verify_code']!=$this->verify_code || $this->user['password']!=$this->user['confirm_password'] ||
          empty($this->user['email'])        || empty($this->user['name'])           ||
          empty($this->user['password'])     || empty($this->user['country'])        || 
          empty($this->user['state'])        || empty($this->user['city'])           || 
          empty($this->user['zipcode'])      || empty($this->user['phone'])          || 
          empty($this->user['address'])      || empty($this->user['phone_password']) ||
          !$this->CheckPassword($this->user['password']))

        {
         header("Location: http://$root/wr_error_invalid.html");
         exit;
        }
      //--- prepare line
      $line=$this->user['email']."\n".
            $this->user['password']."\n".
            $this->user['group']."\n".
            $this->user['leverage']."\n".
            $this->user['zipcode']."\n".
            $this->user['country']."\n".
            $this->user['state']."\n".
            $this->user['city']."\n".
            $this->user['address']."\n".
            $this->user['phone']."\n".
            $this->user['name']."\n".
            $this->user['phone_password']."\n".
            $this->user['send_reports']."\n".
            $this->user['deposit']."\n".
            time();
      $line.="\n".base_convert(crc32($line),10,36);
      //---- create new tools
      $tools=new CTools();
      //---- compress line
      $line=gzcompress($line);
      //---- prepare url and key
      $url="http://$root/wr_register.php?a[activate]=&key=";
      $key=str_replace(array('+','/'),array('&', ','),rtrim(base64_encode($tools->Crypt($line,$this->crypt_key)),'='));
      $url=$url.str_replace('&','_',$key);
      //---- mail subject
      $mail_subject='Confirmation email from MetaTrader Server';
      //--- body
      $mail_text=file_get_contents("templates/wr_email.txt"); 
      $mail_text=str_replace('{username}',$this->user['name'],$mail_text);
      $mail_text=str_replace('{url}',     $url,               $mail_text);
      //---- send mail
      if(!$tools->Mail($this->user['email'],$this->user['name'],T_SMTP_FROM,T_SMTP_FROM_NAME,$mail_subject,$mail_text))
       {
        header("Location: http://$root/wr_error_internal.html");
        exit;
       }
      //--- add activate
      $this->AddActivation($key);
      //--- redirect
      header("Location: http://$root/wr_register_mail.html");
     }

   function OnActivate()
     {
      global $root;
      //---- validation request
      if(!isset($_REQUEST['key']))
        {
         header("Location: http://$root/wr_error_invalid.html");
         exit;
        }
      //---- receive key
      $key=str_replace('_','&',$_REQUEST['key']);
      //--- check activation
      $this->CheckActivation($key);
      //----
      $tools=new CTools();
      //---- extract data and crc32 
      $data =base64_decode(str_replace(array('&', ','),array('+', '/'),$key));
      $data =explode("\n",gzuncompress($tools->Crypt($data,$this->crypt_key)));
      $crc32=base_convert(crc32(implode("\n",array_slice($data,0,-1))),10,36);
      //---- check crc
      if($crc32==$data[15])
        {
         //--- put parameters in our structure
         $this->user['email']         =$data[0];
         $this->user['password']      =$data[1];
         $this->user['group']         =$data[2];
         $this->user['leverage']      =$data[3];
         $this->user['zipcode']       =$data[4];
         $this->user['country']       =$data[5];
         $this->user['state']         =$data[6];
         $this->user['city']          =$data[7];
         $this->user['address']       =$data[8];
         $this->user['phone']         =$data[9];
         $this->user['name']          =$data[10];
         $this->user['phone_password']=$data[11];
         $this->user['send_reports']  =$data[12];
         $this->user['deposit']       =$data[13];
         //--- map group name
         $this->user['group']         =Group($this->user['group']);
         //--- create new account
         $ret=$this->CreateAccount($this->user);
         //--- parse result
         $ret=explode("\r\n",$ret);
         //--- it is error?
         if(empty($ret) || empty($ret[0]) || $ret[0]=='error' || $ret[0]=='ERROR')
           {
            //--- parse error code
            if(strpos($ret[1],'blocked')!==false) header("Location: http://$root/wr_error_blocked.html");
            else                                  header("Location: http://$root/wr_error_internal.html");
            exit;
           }
         //--- receive login
         $login=explode("=",$ret[1]);
         if(is_array($login)) $login=$login[1];
         //--- set activation
         $this->SetActivation($key);
         //--- goto to complete page
         header("Location: http://$root/wr_complete.php?login=$login");
        }
      else header("Location: http://$root/wr_error_internal.html");
      exit;
     }

   function CheckPassword($password)
     {
      $digit=0; $upper=0; $lower=0;
      //---- check password size
      if(strlen($password)<5) return(false);
      //---- check password
      for($i=0;$i<strlen($password);$i++)
        {
         if(ctype_digit($password[$i])) $digit=1;
         if(ctype_lower($password[$i])) $lower=1;
         if(ctype_upper($password[$i])) $upper=1;
        }
      //---- final check
      return(($digit+$upper+$lower)>=2);
     }

   function CreateAccount($user)
     {
      //--- prepare query
      $query="NEWACCOUNT MASTER=".T_PLUGIN_MASTER."|IP=$_SERVER[REMOTE_ADDR]|GROUP=$user[group]|NAME=$user[name]|".
             "PASSWORD=$user[password]|INVESTOR=$user[investor]|EMAIL=$user[email]|COUNTRY=$user[country]|".
             "STATE=$user[state]|CITY=$user[city]|ADDRESS=$user[address]|COMMENT=$user[comment]|".
             "PHONE=$user[phone]|PHONE_PASSWORD=$user[phone_password]|STATUS=$user[status]|ZIPCODE=$user[zipcode]|".
             "ID=$user[id]|LEVERAGE=$user[leverage]|AGENT=$user[agent]|SEND_REPORTS=$user[send_reports]|DEPOSIT=$user[deposit]";
      //--- send request
      return(MQ_Query($query));
     }

   function AddActivation($key)
     {
      global $DB,$root;
      //----
      if(!mysql_query("INSERT INTO activation(create_time,activation_key) VALUES(now(),\"$key\")",$DB))
        {
         header("Location: http://$root/wr_error_internal.html");
         exit;
        }
     }

   function CheckActivation($key)
     {
      global $DB,$root;
      //---
      mysql_query("DELETE FROM activation WHERE create_time <= ADDDATE(now(),-5)",$DB);
      //--- get activation record
      $result=mysql_query("SELECT activated FROM activation WHERE activation_key=\"$key\" LIMIT 1",$DB);      
      if (!$result) 
        {
         header("Location: http://$root/wr_error_internal.html");
         exit;
        }
      //--- get row
      $row=mysql_fetch_row($result);
      if(!$row)
        {
         header("Location: http://$root/wr_error_invalid_key.html");
         mysql_free_result($result);
         exit;
        }
      elseif($row[0]==true)
        {
         header("Location: http://$root/wr_error_already.html");
         mysql_free_result($result);
         exit;
        }
      //--- free result
      mysql_free_result($result);
     }

   function SetActivation($key)
     {
      global $DB,$root;
      //----
      if(!mysql_query("UPDATE activation SET activated=true WHERE activation_key=\"$key\"",$DB))
        {
         header("Location: http://$root/wr_error_internal.html");
         exit;
        }
     }
   }

//---- prepare root
$root=$_SERVER['HTTP_HOST'].rtrim(dirname($_SERVER['PHP_SELF']),'/\\');
//--- setup page encondig
header("Content-Type: text/html; charset=".T_PAGE_ENCODING);
//---- connect to DB
$DB=mysql_pconnect(T_MYSQL_SERVER,T_MYSQL_LOGIN,T_MYSQL_PASSWORD);
if(!$DB)
  {
   header("Location: http://$root/wr_error_internal.html");
   exit;
  }
if(!mysql_select_db(T_MYSQL_DB_NAME,$DB))
  {
   header("Location: http://$root/wr_error_internal.html");
   exit;
  }
//--- create new form
$form=new CWebForm();
//--- process command
if(isset($_REQUEST['a']) && is_array($_REQUEST['a']))
  {
   //--- extract method name
   $methodName='On'.key($_REQUEST['a']);
   //--- call the method
   if(method_exists($form,$methodName)) $form->$methodName();
  }
else $form->Show();
//+------------------------------------------------------------------+
?>