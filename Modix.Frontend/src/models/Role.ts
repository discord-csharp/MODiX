let tinycolor = require("tinycolor2");

export default class Role
{
    id: string = "";
    name: string = "";

    fgColor: string = "";
    //bgColor: string = "";
    
    private _color: string = "";
    get color(): string
    {
        return this._color;
    }

    set color(val: string)
    {
        this._color = val;
        let parsedColor = tinycolor(this.color);

        let determinedColor = parsedColor.darken((21 - tinycolor.readability(parsedColor, "#fff")) * 1.33);

        this.fgColor = determinedColor.toHexString();
    }
}