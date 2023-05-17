#require serdes.fs

1024 constant DMA_SIZE_WORDS
create dma-buffer DMA_SIZE_WORDS cells allot

: dma-buffer-init
  DMA_SIZE_WORDS 0 do
    dup
    i 2 lshift dma-buffer + !
  loop
  ;

$0 dma-buffer-init
dma-buffer serdes-dma-rx-config
SDS_PLL_FREQ_300M serdes-rx-init

: serdes-handler
  R32_SDS_STATUS @
  dup SDS_RX_INT and if
    [char] R emit
    R32_SDS_RX_LEN0 @ hex.
    SDS_RX_INT R32_SDA_STATUS !
  then
  dup SDS_RX_ERR and if
    [char] E emit
    SDS_RX_ERR R32_SDA_STATUS !
  then
  SDS_RX_COMMA_INT and if
    [char] C emit
    SDS_RX_COMMA_INT R32_SDA_STATUS !
  then
;

' serdes-handler irq-serdes !

SDS_RX_INT SDS_COMMA_INT SDS_RX_ERR or or R32_SDS_INT_EN !

SERDES_IRQn pfic-enable-irq

