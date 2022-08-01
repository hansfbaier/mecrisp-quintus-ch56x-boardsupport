\ MB jan 2020
\ dump ( addr len -- )
\ I needed it

decimal

: wrds ( cols -- ) cr  \ A columnar Word list printer. Width = 20 characters, handles overlength Words neatly
   1- >r \ columns
   0                                \ column counter
   dictionarystart
      begin
         dup 8 + dup
         ctype                      \ dup before 6 is for dictionarynext input
         count nip                  \ get number of characters in the word and drop the address of the word
             20 swap - dup 0 > if   \ if Word is less than 20 chars
                   spaces swap      \ pad with spaces to equal 20 chars
                   else drop cr     \ issue immediate carriage return and drop negative number
                   nip -1           \ and reset to column -1
                   then
                      dup r@ = if r@ - cr \ if at 4th column, zero column counter
                      else 1 +
                      then
         swap
         dictionarynext             \   ( a-addr - - a-addr flag )
      until
   2drop
   r> drop
;

: ?emit ( n -- )
    dup 31 >
    over 127 < and \ .s
    IF emit ELSE drop [char] . emit THEN
;

: -aligned ( addr -- addr ' )
    dup 4 mod
    IF 4 - aligned THEN
;

: .dumpline ( addr -- addr' )
    cr
    base @ hex swap
    dup s>d <# # # # # [char] . hold # # # # #> type 3 spaces
    dup
    8 0 DO count s>d <# # # #> type space
    LOOP
    ."  -- "
    8 0 DO count s>d <# # # #> type space
    LOOP
    4 spaces
    swap 8 0 DO count ?emit LOOP
    2 spaces 8 0 DO count ?emit LOOP drop
    swap base !
;

: dump ( addr n -- )
    cr over dup . + swap
    -aligned
    BEGIN .dumpline 2dup <=
    UNTIL
    2drop
;

: inspect ( addr -- )
  begin
    dup $400 dump ( addr -- )
    key           ( addr key -- )
    dup [char] p = if
      swap $800 - swap
      else
      swap $400 + swap
    then
  [char] q = until
  cr
  drop
;