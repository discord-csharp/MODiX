let tinycolor = require("tinycolor2");

export default class Role
{
    id: string = "";
    name: string = "";

    fgColor: string = "";
    bgColor: string = "";
    
    private _color: string = "";
    get color(): string
    {
        return this._color;
    }

    set color(val: string)
    {
        this._color = val;
        let parsedColor = tinycolor(this.color);

        let mixColor = (parsedColor.isLight() ? "#36393f" : "#ffffff");
        this.bgColor = tinycolor.mix(parsedColor, mixColor, 75).toString();

        this.fgColor = parsedColor.lighten(15).toString();
    }
}