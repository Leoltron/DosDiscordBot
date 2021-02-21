import CardModel from "./cardModel";
import CardGroup from "./cardGroup";
import initControls from "./controls";

const deckX = 50;
const deckZ = -100;
const centerRowX = 200;

export default function initGameField(fieldSelector, initialEvents, initialSnapshots, playerNames, currentMove, updateParams) {
    const queue = [];

    function checkQueue() {
        if (queue.length > 0) {
            queue.shift()();
        }

        setTimeout(checkQueue, 100);
    }

    setTimeout(checkQueue, 100);

    function enqueue(action) {
        queue.push(action)
    }

    let events = initialEvents;
    let snapshots = initialSnapshots;
    let currentEventIndex = 0;

    const groups = playerNames.map((n, i) => new CardGroup(i, n, 0, 0, 10 + i, true));

    const awaitingGroups = groups.slice();
    let currentPlayerGroup = null;
    const cards = [];
    let centerRow = [];

    function switchPlayer(newCurrentPlayerId) {
        const prevCurrentPlayer = currentPlayerGroup;
        awaitingGroups.forEach(g => g.z++);
        if (prevCurrentPlayer !== null) {
            if (prevCurrentPlayer.cards.length === 0) {
                prevCurrentPlayer.hideTitle();
            } else {
                prevCurrentPlayer.minified = true;
                prevCurrentPlayer.z = 0;
                awaitingGroups.push(prevCurrentPlayer);
            }
        }
        const newCurrentGroupIndex = awaitingGroups.findIndex(c => c.id === newCurrentPlayerId);
        if (newCurrentGroupIndex === -1) {
            throw new Error("Failed to find player group with id " + newCurrentPlayerId);
        }

        currentPlayerGroup = awaitingGroups.splice(newCurrentGroupIndex, 1)[0];
        readjustCenterRowAndCurrentPlayer();
        readjustAwaitingGroups();
    }

    function switchGroupsCards(group1, group2) {
        [group1.cards, group2.cards] = [group2.cards, group1.cards];
        group1.updateCardPositions();
        group2.updateCardPositions();
    }

    function readjustAwaitingGroups() {
        const centerX = window.innerWidth / 2;
        const x = Math.max(centerX + 150, window.innerWidth - 400);
        let y = 80;
        for (let group of awaitingGroups) {
            group.x = x;
            group.y = y;
            group.minified = true;
            y += 50;
        }
    }

    function addCardToPlayer(cardValue, playerId) {
        addCard(cardValue, deckX, centerRowY, deckZ, false, findGroupById(playerId));
    }

    function addCardToCenterRowFromDeck(cardValue) {
        const card = addCard(cardValue, deckX, centerRowY, deckZ, false);
        setTimeout(() => {
            centerRow.push([card]);
            readjustCenterRowAndCurrentPlayer();
        }, 200);
    }

    function addCardToCenterRowFromGroup(cardValue) {
        const card = currentPlayerGroup.removeCardByValue(cardValue);
        centerRow.push([card]);
        readjustCenterRowAndCurrentPlayer();
    }

    function clearCenterRow() {
        const cardsToClear = centerRow.filter(c => c.length > 1).flatMap(c => c);
        cardsToClear.forEach(c => c.$card.css("opacity", 0));
        centerRow = centerRow.filter(c => c.length === 1);
        readjustCenterRowAndCurrentPlayer();
        setTimeout(() => cardsToClear.forEach(c => c.$card.remove()), 500);
    }

    function matchCard(targetValue, matcherValue1, matcherValue2) {
        const targetRowArray = centerRow.filter(r => r.length === 1 && r[0].shortName === targetValue)[0];
        const matchers = matcherValue2 ? [matcherValue1, matcherValue2] : [matcherValue1];

        matchers.forEach(c => {
            const card = currentPlayerGroup.removeCardByValue(c);
            card.minified = false;
            targetRowArray.push(card);
        });
        readjustCenterRowAndCurrentPlayer();
    }

    function nextEvent() {
        if (queue.length > 0)
            return;
        setTimeout(() => {
            if (currentEventIndex >= events.length)
                return;
            const e = events[currentEventIndex];
            // const s = snapshots[currentEventIndex];
            // console.log("Event " + JSON.stringify(e));
            // console.log("Expected snapshot after event: " + JSON.stringify(s));
            switch (e.eventType) {
                case "PlayerReceivedCard":
                    const player = +e.targetPlayer;
                    e.cards.forEach(c => enqueue(() => addCardToPlayer(c, player)));
                    break;
                case "CenterRowMatch":
                    enqueue(() => matchCard(e.cards[0], e.cards[1], e.cards[2]));
                    break;
                case "CenterRowAdd":
                    e.cards.forEach(c => enqueue(() => addCardToCenterRowFromDeck(c)));
                    break;
                case "CenterRowPlayerAdd":
                    enqueue(() => addCardToCenterRowFromGroup(e.cards[0]));
                    break;
                case "ClearCenterRow":
                    enqueue(clearCenterRow);
                    break;
                case "PlayersSwappedHands":
                    enqueue(() => switchGroupsCards(findGroupById(+e.sourcePlayer), findGroupById(+e.targetPlayer)));
                    break;
                case "PlayerTurnStart":
                    enqueue(() => switchPlayer(+e.sourcePlayer));
                    break;
            }

            currentEventIndex++;
            updateCounter();
        }, 0);
    }

    const $field = $(fieldSelector);
    groups.forEach(g => g.init($field));
    const $deck = $field.find('.dos-card__deck');
    $deck.css('left', deckX + 'px');
    $deck.css('z-index', deckZ + 'px');
    let centerRowY;

    function updateCenterRowY() {
        centerRowY = window.innerHeight / 2 - 71;
    }

    function readjustCenterRowAndCurrentPlayer() {
        updateCenterRowY();
        $deck.css("top", centerRowY + "px");
        let x = centerRowX;
        for (let centerRowGroup of centerRow) {
            for (let i = 0; i < centerRowGroup.length; i++) {
                centerRowGroup[i].x = x;
                centerRowGroup[i].y = centerRowY;
                centerRowGroup[i].z = i;
                x += 40;
            }
            x += 90;
        }

        if (currentPlayerGroup !== null) {
            currentPlayerGroup.y = centerRowY + 150;
            currentPlayerGroup.x = centerRowX;
            currentPlayerGroup.minified = false;
        }
    }

    readjustCenterRowAndCurrentPlayer();
    readjustAwaitingGroups();
    $(window).resize(() => {
        removeAnimations();
        readjustCenterRowAndCurrentPlayer();
        readjustAwaitingGroups();
        restoreAnimations();
    });

    initControls($field, nextEvent, prevEvent);

    const $eventCounter = $field.find('.controls__counter');
    updateCounter();

    function updateCounter() {
        if (events.length === 0)
            $eventCounter.text(`?`);
        else {
            const current = currentEventIndex === undefined ? '?' : currentEventIndex.toString();
            $eventCounter.text(current + '/' + events.length);
        }

        updateURL();
    }

    function addCard(cardValue, firstX = 0, firstY = 0, firstZ = 0, minified = false, nextGroup) {
        const card = new CardModel(cards.length, cardValue, firstX, firstY, firstZ, minified);
        cards.push(card);
        card.init($field);
        if (nextGroup !== undefined) {
            setTimeout(() => {
                if (nextGroup.cards.length === 0) {
                    nextGroup.showTitle();
                    if (awaitingGroups.filter(g => g.id === nextGroup.id).length === 0)
                        awaitingGroups.push(nextGroup);
                }
                nextGroup.addCard(card);
            }, 100);
        }
        return card;
    }

    function findGroupById(id) {
        const group = groups.filter(g => g.id === id)[0];
        if (group === undefined)
            throw new Error("Failed to find group " + id);
        return group;
    }

    function setZeroSnapshot() {
        removeAnimations();
        clearAll();
        cards.splice(0, cards.length);
        groups.forEach(g => g.cards = []);
        groups.forEach(g => g.showTitle());
        awaitingGroups.splice(0, awaitingGroups.length, ...groups);
        currentPlayerGroup = null;
        currentEventIndex = 0;

        readjustCenterRowAndCurrentPlayer();
        readjustAwaitingGroups();
        updateCounter();

        setTimeout(restoreAnimations, 0);
    }

    function setSnapshot(snapshot, nextEventIndex) {
        removeAnimations();
        clearAll();
        cards.splice(0, cards.length);
        groups.forEach(g => g.cards = snapshot.playerHands[g.id].map(cardValue => addCard(cardValue)));
        groups.forEach(g => g.sortCards());
        groups.forEach(g => g.cards.length > 0 ? g.showTitle() : g.hideTitle());

        awaitingGroups.splice(0, awaitingGroups.length, ...groups.filter(g => g.cards.length > 0));

        if (snapshot.currentPlayerId === undefined) {
            currentPlayerGroup = null;
        } else {
            currentPlayerGroup = groups.filter(g => g.id === snapshot.currentPlayerId)[0];
            awaitingGroups.splice(0, awaitingGroups.length, ...awaitingGroups.filter(g => g.id !== snapshot.currentPlayerId));
        }

        centerRow = snapshot.centerRow.map(r => r.map(c => addCard(c)));
        currentEventIndex = nextEventIndex;

        readjustCenterRowAndCurrentPlayer();
        readjustAwaitingGroups();
        updateCounter();

        setTimeout(restoreAnimations, 0);
    }

    function clearAll() {
        $field.find(".dos-card:not(.dos-card__deck)").remove();
        $field.find(".dos-card_minified").remove();
    }

    function restoreAnimations() {
        $field.removeClass('no-animation');
    }

    function removeAnimations() {
        $field.addClass('no-animation');
    }

    function updateURL() {
        if (history.pushState) {
            let newurl = window.location.protocol + "//" + window.location.host + window.location.pathname;
            if (currentEventIndex !== undefined)
                newurl += '?moveId=' + currentEventIndex;
            window.history.pushState({path: newurl}, '', newurl);
        }
    }

    if (currentMove && currentMove > 0 && currentMove < snapshots.length) {
        const snapshotIndex = currentMove - 1;
        const snapshot = snapshots[snapshotIndex];
        setSnapshot(snapshot, snapshotIndex + 1);
    }

    function prevEvent() {
        if (queue.length > 0)
            return;

        setTimeout(() => {
            if (currentEventIndex <= 0)
                return;
            if (currentEventIndex === 1) {
                setZeroSnapshot();
                return;
            }
            const snapshotIndex = currentEventIndex - 2;
            const snapshot = snapshots[snapshotIndex];
            enqueue(() => setSnapshot(snapshot, snapshotIndex + 1));
        }, 0);

    }

}