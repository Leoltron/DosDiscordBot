export default class CardModel {
    constructor(id, shortName, x = 0, y = 0, z = 0, minified = false) {
        this.id = id;
        this.shortName = shortName;
        this._x = x;
        this._y = y;
        this._z = z;
        this._minified = minified;
    }

    cardCssClass() {
        return `${this.minified ? 'dos-card_minified' : 'dos-card'} ${this.isUnknownCard ? 'dos-card_back' : 'dos-card_' + this.cardColor.toLowerCase()}`;
    }

    init($parent) {
        this.$card = $(
            `<div class="${this.cardCssClass()}" id="card-${this.id}" style="top:${this.y}px;left:${this.x}px;z-index:${this.z}">
                ${this.cardText}
            </div>`);

        $parent.append(this.$card);
    }


    get cardText() {
        if (this.isUnknownCard)
            return '?';

        const value = this.shortName.substring(1, this.shortName.length);
        if (value === 'S')
            return '#';
        return value;
    }

    get cardColor() {
        return this.shortName[0];
    }

    set x(value) {
        this._x = value;
        this.$card?.css("left", value + "px");
    }

    get x() {
        return this._x;
    }

    set y(value) {
        this._y = value;
        this.$card?.css("top", value + "px");
    }

    get y() {
        return this._y;
    }

    set z(value) {
        this._z = value;
        this.$card?.css("z-index", value);
    }

    get z() {
        return this._z;
    }

    get isUnknownCard() {
        return this.shortName === '??';
    }


    get minified() {
        return this._minified;
    }

    set minified(minified) {
        this._minified = minified;
        if (this.$card === undefined)
            return;
        const cardMinified = "dos-card_minified";
        const cardFull = "dos-card";
        if (minified) {
            this.$card.addClass(cardMinified);
            this.$card.removeClass(cardFull);
        } else {
            this.$card.removeClass(cardMinified);
            this.$card.addClass(cardFull);
        }
    }
}
