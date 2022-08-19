#require global.fs

\ SERDES registers

\ System: Serdes Analog parameter control register
$40001020 constant R16_SERD_ANA_CFG1  \ RWA, Serdes Analog parameter configuration1
$40001024 constant R32_SERD_ANA_CFG2  \ RWA, Serdes Analog parameter configuration2

$4000B000 constant SERDES_BASE
SERDES_BASE
  #4 + dup constant R32_SDS_CTRL
  #4 + dup constant R32_SDS_INT_EN
  #4 + dup constant R32_SDS_STATUS
  #4 + dup constant R32_SDS_RTX_CTRL
  #4 + dup constant R32_SDS_RX_LEN0
  #4 + dup constant R32_SDS_DATA0
  #4 + dup constant R32_SDS_DMA0
  #4 + dup constant R32_SDS_RX_LEN1
  #4 + dup constant R32_SDS_DATA1
  #4 +     constant R32_SDS_DMA1

\ configuration bits
1  #18 lshift constant SDS_ALIGN_EN	 \ SDS SATA ALIGN Primitive
1  #17 lshift constant SDS_CONT_EN	 \ SDS SATA CONT Primitive
1  #16 lshift constant SDS_POWR_UP
1  #15 lshift constant SDS_TX_PU
1  #14 lshift constant SDS_RX_PU
1  #13 lshift constant SDS_PLL_PU

#21 #8 lshift constant SDS_PLL_FREQ_1_38G   \ 60*(21+2)MHz  (Experimental not officially supported)
#20 #8 lshift constant SDS_PLL_FREQ_1_32G   \ 60*(20+2)MHz  (Experimental not officially supported)
#19 #8 lshift constant SDS_PLL_FREQ_1_26G   \ 60*(19+2)MHz  (Experimental not officially supported)
#18 #8 lshift constant SDS_PLL_FREQ_1_20G   \ 60*(18+2)MHz  => Official SerDes Speed 1.2Gbps
#17 #8 lshift constant SDS_PLL_FREQ_1_14G   \ 60*(17+2)MHz
#16 #8 lshift constant SDS_PLL_FREQ_1_08G   \ 60*(16+2)MHz
#15 #8 lshift constant SDS_PLL_FREQ_1_02G   \ 60*(15+2)MHz
#14 #8 lshift constant SDS_PLL_FREQ_960M    \ 60*(14+2)MHz
#13 #8 lshift constant SDS_PLL_FREQ_900M    \ 60*(13+2)MHz
 #8 #8 lshift constant SDS_PLL_FREQ_600M    \ 60*( 8+2)MHz
 #7 #8 lshift constant SDS_PLL_FREQ_540M    \ 60*( 7+2)MHz
 #6 #8 lshift constant SDS_PLL_FREQ_480M    \ 60*( 6+2)MHz
 #5 #8 lshift constant SDS_PLL_FREQ_420M    \ 60*( 5+2)MHz
 #4 #8 lshift constant SDS_PLL_FREQ_360M    \ 60*( 4+2)MHz
 #3 #8 lshift constant SDS_PLL_FREQ_300M    \ 60*( 3+2)MHz
 #2 #8 lshift constant SDS_PLL_FREQ_240M    \ 60*( 2+2)MHz  (Experimental not officially supported)
 #1 #8 lshift constant SDS_PLL_FREQ_180M    \ 60*( 1+2)MHz  (Experimental not officially supported)

1 #7 lshift constant SDS_DMA_EN
1 #6 lshift constant SDS_TX_EN
1 #5 lshift constant SDS_RX_EN
1 #4 lshift constant SDS_RX_POLAR
1 #3 lshift constant SDS_INT_BUSY_EN
1 #2 lshift constant PHY_RESET
1 #1 lshift constant LINK_RESET
1 #0 lshift constant SDS_ALL_CLR

\ SDS_RTX_CTRL
1 #16 lshift constant SDS_LINK_INIT
1 #17 lshift constant SDS_TX_START
1 #18 lshift constant SDS_BUF_MODE	 \ (Double DMA Buffer Mode)

\ SDS->SDS_INT_EN => SerDes_EnableIT()
\ SDS->SDS_STATUS => SerDes_Status() / SerDes_ClearIT()
1 #0 lshift constant SDS_PHY_RDY
1 #1 lshift constant SDS_TX_INT
1 #1 lshift constant SDS_RX_ERR
1 #2 lshift constant SDS_RX_INT
1 #3 lshift constant SDS_FIFO_OV
1 #5 lshift constant SDS_COMMA_INT  \ Seems to do not work Triggered 500us before data ...
#47         constant ALL_INT


1 #17 lshift constant SDS_SEQ_MATCH
1 #18 lshift constant SDS_RX_CRC_OK
1 #19 lshift constant SDS_PLL_READY
1 #20 lshift constant SDS_TX_READY

#1 #24 lshift constant SDS_RX_BUF		 \ Not documented/not used
#1 #28 lshift constant SDS_TX_BUF		 \ Not documented/not used
#1 #20 lshift constant SDS_TX_NUMP_1	 \ Not documented/not used
#2 #20 lshift constant SDS_TX_NUMP_2	 \ Not documented/not used
#3 #20 lshift constant SDS_TX_NUMP_3	 \ Not documented/not used
#4 #20 lshift constant SDS_TX_NUMP_4	 \ Not documented/not used

: _serdes-basic-init ( SDS_PLL_FREQ -- )
  safe-access-mode-on
  $5a       R16_SERD_ANA_CFG1 h!
  $00423015 R32_SERD_ANA_CFG2  !
  >r

  PHY_RESET LINK_RESET SDS_ALL_CLR or or
  R32_SDS_CTRL !
  1 ms

  SDS_POWR_UP SDS_TX_PU SDS_RX_PU r> LINK_RESET SDS_ALL_CLR or or or or or
  R32_SDS_CTRL !

  begin
    R32_SDS_STATUS @
    SDS_PLL_READY and
  until

  1 ms
;

: _serdes-wait-flag ( flag -- )
  begin
    dup
    R32_SDS_STATUS @
    and
  until
  drop
;

: serdes-tx-init ( SDS_PLL_FREQ -- )
  dup _serdes-basic-init
  >r

  SDS_POWR_UP      SDS_TX_PU   SDS_RX_PU  SDS_PLL_PU    r>           or or or or
  SDS_INT_BUSY_EN  SDS_DMA_EN  SDS_TX_EN  SDS_ALIGN_EN  SDS_CONT_EN  or or or or
  or
  R32_SDS_CTRL !

  SDS_LINK_INIT R32_SDS_RTX_CTRL !
  1 ms

  0 R32_SDS_RTX_CTRL !

  SDS_TX_READY _serdes-wait-flag
;

: serdes-rx-init ( SDS_PLL_FREQ )
  dup _serdes-basic-init
  >r

  SDS_POWR_UP SDS_TX_PU SDS_RX_PU SDS_PLL_PU r>   or or or or
  SDS_DMA_EN  SDS_RX_EN or
  or
  R32_SDS_CTRL !
;

: serdes-dma-tx-config ( dma-addr tx-len user-data -- )
  SDS_TX_READY _serdes-wait-flag

  R32_SDS_DATA0    !
  R32_SDS_RTX_CTRL !
  R32_SDS_DMA0     !
;

: serdes-dma-tx ( -- )
  SDS_TX_READY _serdes-wait-flag

  SDS_TX_START R32_SDS_RTX_CTRL bis!
;

: serdes-wait-tx-done
  SDS_TX_INT _serdes-wait-flag

  SDS_TX_START R32_SDS_RTX_CTRL bic!
  begin
    R32_SDS_RTX_CTRL @
    SDS_TX_START and
    not
  until

  SDS_TX_INT R32_SDS_STATUS !
;

: serdes-wait-comma SDS_COMMA_INT _serdes-wait-flag ;

: serdes-dma-rx-config ( dma-addr -- )
    R32_SDS_DMA0     !
  0 R32_SDS_RTX_CTRL !
;

: serdes-doubledma-rx-config ( dma-addr0 dma-addr1 -- )
  R32_SDS_DMA1 !
  R32_SDS_DMA0 !
  SDS_BUF_MODE R32_SDS_RTX_CTRL !
;