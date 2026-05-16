using Common;
using ParkanPlayground;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var converter = new MshConverter();

converter.Convert("E:\\ParkanUnpacked\\fortif.rlb\\130_fr_b_plant.msh");