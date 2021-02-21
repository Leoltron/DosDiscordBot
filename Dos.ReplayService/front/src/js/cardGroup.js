const textClass = "card-group__text";
const textClassMinified = "card-group__text_minified";

export default class CardGroup {
    constructor(id, title, x, y, z, minified = false) {
        this.id = id;
        this.cards = [];
        this.title = title;
        this._x = x;
        this._y = y;
        this._z = z;
        this._minified = minified;
    }

    init($parent) {
        this.$title = $(`<div class="${this.minified ? textClassMinified : textClass}" style="top:${this.getTextY()}px;left:${this._x}px">${this.title}</div>`);
        $parent.append(this.$title);
        this.updateCardPositions();
    }

    getTextY() {
        return this.y + (this.minified ? 10 : 49);
    }

    get x() {
        return this._x;
    }

    set x(newX) {
        this._x = newX;
        this.updateCardPositions();
    }

    updateTextY() {
        this.$title.css('top', this.getTextY() + 'px');
    }

    updateCardPositions() {
        let x = this.x;
        for (let card of this.cards) {
            card.x = x;
            card.y = this.y;
            card.z = this.z;
            card.minified = this.minified;
            x += this.minified ? 23 : 90;
        }
        this.$title.css('left', x + 'px');
    }

    get y() {
        return this._y;
    }

    set y(newY) {
        this._y = newY;
        this.cards.forEach(c => c.y = newY);
        this.updateTextY();
    }

    get z() {
        return this._z;
    }

    set z(newZ) {
        this._z = newZ;
        this.cards.forEach(c => c.z = newZ);
    }

    get minified() {
        return this._minified;
    }

    set minified(newMinified) {
        this._minified = newMinified;
        this.cards.forEach(c => c.minified = newMinified);
        if (newMinified) {
            this.$title.addClass(textClassMinified);
            this.$title.removeClass(textClass);
        } else {
            this.$title.addClass(textClass);
            this.$title.removeClass(textClassMinified);
        }
        this.updateTextY();
        this.updateCardPositions();
    }

    addCard(card) {
        this.cards.push(card);
        card.minified = this.minified;
        card.y = this.y;
        card.z = this.z;
        this.sortCards();
    }

    sortCards() {
        this.cards.sort((a, b) => compareColors(a.cardColor, b.cardColor) || (a.cardNumber - b.cardNumber));
        this.updateCardPositions();
    }

    removeCardByValue(value) {
        const cardToRemove = this.cards.filter(c => c.shortName === value)[0];
        if (cardToRemove === undefined)
            throw new Error(`Failed to find card "${value}" in group "${this.title}" (id: ${this.id})`);
        this.cards = this.cards.filter(c => c.id !== cardToRemove.id);
        this.updateCardPositions();
        return cardToRemove;
    }

    hideTitle() {
        this.$title.css("opacity", 0);
    }

    showTitle() {
        this.$title.css("opacity", 1);
    }
}

function compareColors(a, b) {
    return colorPriority(a) - colorPriority(b);
}

function colorPriority(color) {
    switch (color) {
        case "R":
            return 1;
        case "G":
            return 2;
        case "Y":
            return 4;
        case "B":
            return 8;
        case "W":
            return 15;
        default:
            return 0;
    }
}