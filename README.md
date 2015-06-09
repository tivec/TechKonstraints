# TechKonstraints

This plugin adds part purchase requirements prior to purchasing parts in the R&D. Part purchase-ception!

Obviously, if you have disabled part purchase in the career, you really don't need this plugin.

For mod developers, add the following to your part configuration:
```
requiredParts = solidBooster,fuelTankSmallFlat
```

You could use ModuleManager to set the part requirements for other parts like this:

```
@PART[liquidEngine]
{
	// Make the LV-T30 require the RT-10 SRB and the FL-T100 before purchase can be completed.
	requiredParts = solidBooster,fuelTankSmallFlat
}

@PART[liquidEngine2]
{
	// Make the LV-T45 require the
	requiredParts = liquidEngine
}
```

## License

The plugin uses the AstrotechUtilities library, available under GPLv3 on https://github.com/tivec/AstrotechUtilities

