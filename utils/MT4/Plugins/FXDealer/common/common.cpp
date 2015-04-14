//+------------------------------------------------------------------+
//|                                        MetaTrader Virtual Dealer |
//|                 Copyright © 2001-2008, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#include "..\stdafx.h"
//----
static const double ExtDecimalArray[9] ={ 1.0, 10.0, 100.0, 1000.0, 10000.0,100000.0, 1000000.0, 10000000.0, 10000000.0 };  // положительные степени 10
//+------------------------------------------------------------------+
//|  ”довлетвор€ет ли строка переданному фрагменту рег.выражени€?    |
//+------------------------------------------------------------------+
int CheckTemplate(char* expr,char* tok_end,char* group,char* prev,int* deep)
  {
   char  tmp=0;
   char *lastwc,*prev_tok,*cp;
//---- проверим глубину рекурсии
   if((*deep)++>=10) return(FALSE);
//---- пропускаем повторы *
   while(*expr=='*' && expr!=tok_end) expr++;
   if(expr==tok_end) return(TRUE);
//---- ищем следующую звездочку или конец
   lastwc=expr;
   while(*lastwc!='*' && *lastwc!=0) lastwc++;
//---- временно ограничиваем строку
   if((tmp=*(lastwc))!=0) // токен не последний в строке?
     {
      tmp=*(lastwc);*(lastwc)=0;
      if((prev_tok=strstr(group,expr))==NULL) { if(tmp!=0) *(lastwc)=tmp; return(FALSE); }
      *(lastwc)=tmp;
     }
   else // токен последний...
     {
      //---- провер€ем
      cp=group+strlen(group);
      for(;cp>=group;cp--)
        if(*cp==expr[0] && strcmp(cp,expr)==0)
          return(TRUE);
      return(FALSE);
     }
//---- нарушен пор€док?
   if(prev!=NULL &&  prev_tok<=prev) return(FALSE);
   prev=prev_tok;
//----
   group=prev_tok+(lastwc-expr-1);
//---- дошли до конца?
   if(lastwc!=tok_end) return CheckTemplate(lastwc,tok_end,group,prev,deep);
//----
   return(TRUE);
  }
//+------------------------------------------------------------------+
//|  ”довлетвор€ет ли группа одному из шаблонов?                     |
//+------------------------------------------------------------------+
int CheckGroup(char* grouplist, const char *_group)
{
	if(grouplist==NULL || _group==NULL) return(FALSE);

	char *group = new char [256];
	strcpy(group, _group);
	// проверки
   
	// проходимс€ по всем группам
	char *tok_start=grouplist,end;
	int  res=TRUE,deep=0,normal_mode;
	while(*tok_start!=0)
    {
      //---- пропустим зап€тые
      while(*tok_start!=0 && *tok_start==',') tok_start++;
      //----
      if(*tok_start=='!') { tok_start++; normal_mode=FALSE; }
      else                 normal_mode=TRUE;
      //---- найдем границы токена
      char *tok_end=tok_start;
      while(*tok_end!=',' && *tok_end!=0) tok_end++;
      end=*tok_end; *tok_end=NULL;
      //----
      char *tp=tok_start;
      char *gp=group, *prev=NULL;
      //---- проходим по токену
      res=TRUE;
      while(tp!=tok_end && *gp!=NULL)
        {
         //---- нашли звЄздочку? провер€ем как регэксп
         if(*tp=='*')
           {
            if((res=CheckTemplate(tp,tok_end,gp,prev,&deep))==TRUE)
              {
               *tok_end=end;
			   delete []group;
               return(normal_mode);
              }
            break;
           }
         //---- просто провер€ем
         if(*tp!=*gp) { *tok_end=end; res=FALSE; break; }
         tp++; gp++;
        }
      //---- восстанавливаем
      *tok_end=end;
      if(*gp==NULL && tp==tok_end && res==TRUE) 
	  {
		  delete []group;
		  return(normal_mode);
	  }
      //---- переход к следующему токену
      if(*tok_end==0) break;
      tok_start=tok_end+1;
     }
//----
   delete []group;
   return(FALSE);
  }
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
